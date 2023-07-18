using System.Reflection;
using Debank;
using Leaf.xNet;
using MemoryPack;
using Newtonsoft.Json;
using ZstdSharp;

namespace CryptoEat.Modules.Scanners;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public static class TronCache
{
    [Obfuscation(Feature = "renaming", ApplyToMembers = true)] [JsonProperty]
    public static Dictionary<string, TronResult> CacheDictionary = new();
}

internal class TronScan : IDisposable
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

        var path = Path.Combine(dir, "cache2.bin");
        try
        {
            var bin = MemoryPackSerializer.Serialize(TronCache.CacheDictionary);
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

        var path = Path.Combine(dir, "cache2.bin");
        if (!File.Exists(path))
            return;
        try
        {
            var bin = File.ReadAllBytes(path);
            using var decompressor = new Decompressor();
            bin = decompressor.Unwrap(bin).ToArray();

            TronCache.CacheDictionary =
                MemoryPackSerializer.Deserialize<Dictionary<string, TronResult>>(bin)!;
        }
        catch (Exception ex)
        {
            if (Generic.DEBUG) Console.WriteLine(ex.ToString());

            Generic.WriteError(ex);
        }
    }

    private TronResult GetTron(string adr)
    {
        RotateProxy();
        return JsonConvert.DeserializeObject<TronResult>(request.Get($"https://apilist.tronscan.org/api/account/token_asset_overview?address={adr}").ToString()!)!;
    }

    internal TronResult CachedGetTokens(string address)
    {
        if (TronCache.CacheDictionary.TryGetValue(address, out var tokens)) return tokens;

        repeat:
        try
        {
            var resp = GetTron(address);
            TronCache.CacheDictionary.Add(address, resp);
            return resp;
        }
        catch
        {
            RotateProxy();
            goto repeat;
        }
    }
}

[MemoryPackable]
[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public partial class TronResult
{
    [JsonProperty("totalAssetInTrx")] public decimal TotalAssetInTrx { get; set; }

    [JsonProperty("totalTokenCount")] public long TotalTokenCount { get; set; }

    [JsonProperty("totalAssetInUsd")] public decimal TotalAssetInUsd { get; set; }
}
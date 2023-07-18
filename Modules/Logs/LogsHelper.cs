using System.Text.RegularExpressions;

namespace CryptoEat.Modules.Logs;

internal class LogsHelper
{
    private static readonly Regex LoginPassRegex = new(@"(?i)(user(?:name)?|login):\s*(.*?)\s*(pass(?:word)?):?\s*(.*)",
        RegexOptions.Compiled);
    private static readonly Regex Regex4 = new("Value: (.*)", RegexOptions.Compiled);
    private static readonly Regex Regex5 = new(@".*\t(.*)", RegexOptions.Compiled);
    private static readonly Regex Regex7 = new(@"\| Text: (.*)", RegexOptions.Compiled);
    private static readonly Regex Regex8 = new(@"(\t| )(.*)", RegexOptions.Compiled);
    private static readonly Regex Regex9 = new(" --==> (.*)", RegexOptions.Compiled);

    private string? _path;
    internal HashSet<string>? AutoFills;
    internal HashSet<string>? Passwords;
    internal HashSet<string>? Users;

    internal LogsHelper(string? path)
    {
        _path = path;

        Init();

        Users?.RemoveWhere(x => x.Length < 4);
        Passwords?.RemoveWhere(x => x.Length < 4);
    }

    private void Init()
    {
        _path ??= string.Empty;
        Passwords ??= new HashSet<string>();
        Users ??= new HashSet<string>();
        AutoFills = GetAutoFills(_path);

        var dirInfo = new DirectoryInfo(_path);
        for (var i = 0; i < 7; i++)
        {
            if (dirInfo is {Exists: false})
            {
                break;
            }

            // hope its workin'
            if (dirInfo != null)
            {
                var files = dirInfo.EnumerateFiles().ToList();

                if (!files.Any(x =>
                        x.Name.Contains("password", StringComparison.OrdinalIgnoreCase) &&
                        Path.GetExtension(x.FullName) == ".txt"))
                {
                    dirInfo = dirInfo?.Parent;
                    continue;
                }

                try
                {
                    var passFiles = files.Where(x =>
                        x.Name.Contains("password", StringComparison.OrdinalIgnoreCase) &&
                        Path.GetExtension(x.FullName) == ".txt");
                    foreach (var passFile in passFiles)
                    {
                        var matches = LoginPassRegex.Matches(File.ReadAllText(passFile.FullName)
                            .Replace("\r", ""));
                        if (matches.Count <= 0)
                        {
                            continue;
                        }

                        Passwords.UnionWith(matches
                            .Select(match => match.Groups[4].Value.Replace("\n", "").Replace("\r", ""))
                            .Distinct());
                        Users.UnionWith(matches
                            .Select(match => match.Groups[2].Value.Replace("\n", "").Replace("\r", ""))
                            .Distinct());
                    }
                }
                catch (Exception ex)
                {
                    Generic.WriteError(ex);
                }
                

                if (Directory.Exists(Path.Combine(dirInfo.FullName, "Browsers")))
                {
                    dirInfo = new DirectoryInfo(Path.Combine(dirInfo.FullName, "Browsers"));
                }
            }
        }
    }
    #region Dirty AutoFills
    private static HashSet<string> GetAutoFills(in string dir)
    {
        var dirInfo = new DirectoryInfo(dir);
        var autofills = new HashSet<string>();
        for (var i = 0; i < 10; i++)
        {
            if (dirInfo == null)
            {
                break;
            }

            AddAutoFillsFromDirectory(dirInfo.FullName, "browsers", "*autofill.txt", ProcessAutoFillType1,
                ref autofills);
            AddAutoFillsFromDirectory(dirInfo.FullName, "browsers/autofills", "*.txt", ProcessAutoFillType1,
                ref autofills); // racoon
            AddAutoFillsFromDirectory(dirInfo.FullName, "Autofills", null, ProcessAutoFillType2, ref autofills);
            AddAutoFillsFromDirectory(dirInfo.FullName, "Autofill", null,
                ProcessAutoFillType3, ref autofills);
            AddAutoFillsFromDirectory(dirInfo.FullName, "Autofill", null,
                ProcessAutoFillType4, ref autofills);

            AddAutoFillsFromFile(Path.Combine(dirInfo.FullName, "Browsers", "AutoFill.txt"),
                ProcessAutoFillType5, ref autofills);
            AddAutoFillsFromFile(Path.Combine(dirInfo.FullName, "_AllForms_list.txt"),
                ProcessAutoFillType6, ref autofills);

            dirInfo = dirInfo.Parent;
        }

        return autofills
            .Where(x => x.Length is > 3 and < 45)
            .ToHashSet();
    }

    private static void AddAutoFillsFromDirectory(string? basePath, string subPath, string? searchPattern,
        Func<string, IEnumerable<string>> processAutoFill, ref HashSet<string> autofills)
    {
        if (basePath == null)
        {
            return;
        }

        var fullPath = Path.Combine(basePath, subPath);
        if (!Directory.Exists(fullPath))
        {
            return;
        }

        searchPattern ??= "*";

        foreach (var file in Directory.EnumerateFiles(fullPath, searchPattern))
        {
            try
            {
                autofills.UnionWith(processAutoFill(File.ReadAllText(file.Replace("\\", "/")).Replace("\r", "")));
            }
            catch (Exception e)
            {
                Generic.WriteError(e);
            }
        }
    }

    private static void AddAutoFillsFromFile(string filePath, Func<string, IEnumerable<string>> processAutoFill,
        ref HashSet<string> autofills)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        try
        {
            autofills.UnionWith(processAutoFill(File.ReadAllText(filePath).Replace("\r", "")));
        }
        catch (Exception e)
        {
            Generic.WriteError(e);
        }
    }

    private static IEnumerable<string> ProcessAutoFillType1(string fileContent) =>
        fileContent.Split('\n')
            .Where(x => x is not "" or "\n" or "\r\n" or "\r")
            .Where((_, i) => i % 2 == 1)
            .Distinct();

    private static IEnumerable<string> ProcessAutoFillType2(string fileContent) =>
        Regex4.Matches(fileContent)
            .Select(x => x.Groups[1].Value)
            .Distinct();

    private static IEnumerable<string> ProcessAutoFillType3(string fileContent) =>
        Regex5.Matches(fileContent)
            .Select(x => x.Groups[1].Value)
            .Distinct();

    private static IEnumerable<string> ProcessAutoFillType4(string fileContent) =>
        Regex8.Matches(fileContent)
            .Select(x => x.Groups[2].Value)
            .Distinct();

    private static IEnumerable<string> ProcessAutoFillType5(string fileContent) =>
        Regex7.Matches(fileContent)
            .Select(x => x.Groups[1].Value)
            .Distinct();

    private static IEnumerable<string> ProcessAutoFillType6(string fileContent) =>
        Regex9.Matches(fileContent)
            .Select(x => x.Groups[1].Value)
            .Distinct();
    #endregion
}
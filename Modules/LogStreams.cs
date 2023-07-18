using System.Text.RegularExpressions;

namespace CryptoEat.Modules;

internal static class LogStreams
{
    internal static StreamWriter ErrorLog;
    internal static List<string> ErrorsList = new();

    internal static StreamWriter ScanBrutedLog;
    internal static StreamWriter ScanLog;

    private static StreamWriter MnemoResultsLog;
    private static StreamWriter MnemoAllLog;

    internal static StreamWriter AllPasswordsLog;

    internal static StreamWriter AutoSwapLog;
    internal static StreamWriter AutoWithdrawLog;

    internal static void Initialize()
    {
        var pathToLog = Path.Combine("Results", DateTime.Now.ToString("yyyy_MM_dd"), DateTime.Now.ToString("HH_m_s"));
        if (!Directory.Exists(pathToLog)) Directory.CreateDirectory(pathToLog);

        ErrorLog = GenerateStreamWriter("errors.log");
        MnemoAllLog = GenerateStreamWriter(Path.Combine("Results", "all_mnemos.txt"));
        ScanBrutedLog = GenerateStreamWriter(Path.Combine(pathToLog, "results_bruted.txt"));
        ScanLog = GenerateStreamWriter(Path.Combine(pathToLog, "results.txt"));
        MnemoResultsLog = GenerateStreamWriter(Path.Combine(pathToLog, "mnemos.txt"));
        AutoSwapLog = GenerateStreamWriter(Path.Combine(pathToLog, "autoswap.txt"));
        AutoWithdrawLog = GenerateStreamWriter(Path.Combine(pathToLog, "autowithdraw.txt"));
    }

    private static StreamWriter GenerateStreamWriter(string path)
    {
        return new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true
        };
    }

    internal static void InitializeAfterSettings()
    {
        AllPasswordsLog = GenerateStreamWriter("all_passwords.txt");
    }

    internal static void WriteMnemo(string? mnemo)
    {
        MnemoAllLog.WriteLine(mnemo);
        MnemoResultsLog.WriteLine(mnemo);

        MnemoAllLog.Flush();
        MnemoResultsLog.Flush();
    }

    internal static void DisposeMnemo()
    {
        MnemoAllLog.Flush();
        MnemoAllLog.Close();

        MnemoResultsLog.Flush();
        MnemoResultsLog.Close();
    }

    internal static void WriteLog(string log)
    {
        ScanLog.WriteLine(log.Clear());
        ScanLog.Flush();
    }

    internal static void WriteBrutedLog(string log)
    {
        ScanBrutedLog.WriteLine(log.Clear());
        ScanBrutedLog.Flush();
    }
}

public static partial class StringExt
{
    public static string Clear(this string str)
    {
        return ClearReg().Replace(str, "");
    }

    public static string Replace(this string str, string[] value, string replacement)
    {
        return value.Aggregate(str, (current, s) => current.Replace(s, replacement));
    }

    public static string Replace(this string str, char[] value, char replacement)
    {
        return value.Aggregate(str, (current, s) => current.Replace(s, replacement));
    }

    public static string Replace(this string str, char[] value, string replacement)
    {
        return value.Aggregate(str, (current, s) => current.Replace(s.ToString(), replacement));
    }

    [GeneratedRegex(@"\e\[[0-9;]*m(?:\e\[K)?", RegexOptions.Compiled)]
    private static partial Regex ClearReg();
}
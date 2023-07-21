using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CryptoEat.Modules.HelpersN;
using CryptoEat.Modules.Logs;
using CryptoEat.Modules.Models;
using CryptoEat.Modules.Network;
using CryptoEat.Modules.Scanners;
using Debank;
using Newtonsoft.Json;
using Pastel;

namespace CryptoEat.Modules;

internal static partial class Generic
{
    internal static void GreetUser()
    {
        Console.Write("Hello, ", Color.Coral);
        Console.Write(Environment.UserName, Color.LightCoral);
        Console.WriteLine("!", Color.Coral);
    }

    internal static void RequestPath()
    {
        Console.Write(
            "Enter the path to the folder, you can just drag it to this window, and then press '",
            Color.Coral);
        Console.Write("Enter", Color.LightCoral);
        Console.WriteLine("' ;)", Color.Coral);

        while (Path.Length == 0)
        {
            var newpath = Console.ReadLine()
                .Replace("\"", "")
                .Replace("\'", "")
                .Replace("\n", "")
                .Replace("\r", "");

            if (newpath.Length != 0)
            {
                if (Directory.Exists(newpath))
                {
                    Path = newpath;
                    break;
                }

                if (File.Exists(newpath))
                    try
                    {
                        var dirpath = new FileInfo(newpath).DirectoryName;
                        if (dirpath is not null && !string.IsNullOrEmpty(dirpath))
                        {
                            Console.WriteLine(
                                "It looks like you dragged the file or entered the path to the file. Can I use the folder containing it? If yes, press space",
                                Color.LightGreen);
                            if (Console.ReadKey().Key == ConsoleKey.Spacebar)
                            {
                                Path = dirpath;
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // ignore
                    }
            }

            Console.Write(
                "This folder either does not exist, or you entered an empty line!\nTry again, enter the path or drag the folder into the window and press '",
                Color.DeepPink);
            Console.Write("Enter", Color.HotPink);
            Console.WriteLine("' >.<", Color.DeepPink);
        }
    }

    internal static void SetPriority()
    {
        Thread.CurrentThread.Priority = ThreadPriority.Highest;
        Process.GetCurrentProcess().PriorityClass = !DEBUG ? ProcessPriorityClass.RealTime : ProcessPriorityClass.High;
        SetRealtimePriority();
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetCurrentProcess();

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial void SetPriorityClass(IntPtr handle, uint priorityClass);

    internal static void SetRealtimePriority()
    {
        try
        {
            SetPriorityClass(GetCurrentProcess(), 0x00000100);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    internal static void ProcessPath()
    {
        Console.Clear();
        Console.WriteLine("I start collecting folders - we'll have to wait...", Color.Coral);
        Helpers.WriteTip("Information about the current progress can be seen in the header of the window");

        DirsList = FileSystem.ScanDirectories(Path, true)
            .ToList();
        Helpers.SetTitle();
    }

    internal static void LoadSettings()
    {
        try
        {
            if (File.Exists("settings.json"))
            {
                Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json")) ?? Settings;
                InitSettings();
            }
            else
            {
                Console.WriteLine("Missing settings.json. Contact the developer.", Color.DeepPink);
                Console.ReadLine();

                Environment.Exit(-1);
                Environment.FailFast("set_not");
            }
        }
        catch (Exception e)
        {
            WriteError(e);
            Console.WriteLine("Incorrect settings. Contact the developer.", Color.DeepPink);
            Console.ReadLine();

            Environment.Exit(-1);
            Environment.FailFast("set_err");
        }
    }

    internal static void InitSettings()
    {
        var specialSymbols = "!@&$?.;*_0123456789".ToCharArray().ToList();
        if (Settings.BruteLevel > 1) specialSymbols.AddRange("#%^()-=+"); // MEDIUM

        if (Settings.BruteLevel > 2) specialSymbols.AddRange("qwertyuiopasdfghjkl:'\\|zxcvbnm"); // HARD

        if (Settings.BruteLevel > 3)
            specialSymbols.AddRange("QWERTYUIOPASDFGHJKLZXCVBNM\"`~/<>, {}[]"); // ULTRA HARD [IMPOSSIBLE]

        PassGen.Addictions = specialSymbols.Distinct().ToArray();

        if (File.Exists("brute.txt"))
        {
            if (Settings.BruteTopPercent > 0)
            {
                var file = File.ReadAllLines("brute.txt");
                Settings.BruteTopCount =
                    (int)Math.Round(file.Length * Settings.BruteTopPercent / 100d);
                PassGen.BruteTopList = file
                    .Take(Settings.BruteTopCount)
                    .Select(x => x.Replace("\r", ""))
                    .Where(x => x.Length > 7)
                    .ToHashSet();
            }
            else
            {
                Settings.BruteTopCount = 0;
            }
        }

        if (File.Exists("all_passwords.txt"))
            PassGen.PreviousPasswords = File.ReadLines("all_passwords.txt")
                .Select(x => x.Replace("\r", ""))
                .ToHashSet();

        TempDir = System.IO.Path.Combine(Environment.CurrentDirectory, "temp", $"sessions{Guid.NewGuid()}");
        if (!Directory.Exists(TempDir))
            Directory.CreateDirectory(TempDir);

        if (File.Exists(Settings.ProxyPath) && Settings is { ProxyFormat: not null, Scan: true })
        {
            var tempProxy = File.ReadAllLines(Settings.ProxyPath)
                .Select(x => x.Replace(new[] { '\r', '\n', ' ' }, ""))
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            if (!tempProxy.Any())
            {
                Console.WriteLine("No proxy!", Color.DeepPink);
                Console.ReadLine();

                Environment.Exit(-1);
                Environment.FailFast("no_prox");
            }

            var tempList = new List<Proxy>();

            foreach (var proxy in tempProxy)
                try
                {
                    var loaded = Proxy.InitFromString(proxy, Settings.ProxyFormat);
                    if (loaded != null) tempList.Add(loaded);
                }
                catch (Exception e)
                {
                    WriteError(e);
                }

            Helpers.SetTitle($"Checking {tempList.Count} proxies");
            Helpers.Max = tempList.Count;

            tempList = tempList
                .AsParallel()
                .WithDegreeOfParallelism(tempList.Count > 50 ? 50 : tempList.Count)
                .Where(x => x.Check())
                .ToList();

            tempList = tempList.Randomize();

            Helpers.ResetCounters();
            Helpers.SetTitle();

            if (!tempList.Any())
            {
                Console.WriteLine("No proxy!", Color.DeepPink);
                Console.ReadLine();

                Environment.Exit(-1);
                Environment.FailFast("no_prox");
            }

            Console.WriteLine($"Loaded {tempList.Count.ToString().Pastel(Color.LightCoral)} proxies");
            ProxyList = new EnumeratorWrapper<Proxy>(tempList);
            ProxyCount = tempList.Count;
        }
        else if (Settings.Scan)
        {
            Console.WriteLine("No proxy!", Color.DeepPink);
            Console.ReadLine();

            Environment.Exit(-1);
            Environment.FailFast("no_prox");
        }
    }

    internal static async Task CheckAntipublicAccess()
    {
        if (string.IsNullOrEmpty(Settings.AntiPublicKey))
        {
            Settings.AntipublicWorking = false;
            return;
        }

        Helpers.SetTitle("Checking Antipublic Plus access...");

        try
        {
            using var api = new MyrzApi(Settings.AntiPublicKey);
            var result = await api.CheckAccess();
            Settings.AntipublicWorking = result.plus;
        }
        catch (Exception e)
        {
            WriteError(e);
            Settings.AntipublicWorking = false;
        }

        Helpers.SetTitle();
    }

    internal static void WriteError(Exception e)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"[{DateTime.Now:g} CryptoEat error]:");
        sb.AppendLine(e.ToString());
        if (e.InnerException is not null)
        {
            sb.AppendLine($"\tInner Exception: [{e.InnerException}]");
            if (e.InnerException.InnerException is not null)
                sb.AppendLine($"\t\tInner Exception: [{e.InnerException.InnerException}]");
        }

        sb.AppendLine();

        var result = sb.ToString();
        if (LogStreams.ErrorsList.Contains(result)) return;

        LogStreams.ErrorsList.Add(result);
        LogStreams.ErrorLog.WriteLine(result);
        LogStreams.ErrorLog.Flush();
    }

    internal static void SetOnClose()
    {
        ProcessExit = (_, _) => { OnExit(); };
        AppDomain.CurrentDomain.ProcessExit += ProcessExit;
    }

    internal static void OnExit()
    {
        DeBank.SaveCache();
        TronScan.SaveCache();

        Helpers.ResetCounters();
        Helpers.SetTitle("Please, wait...");
        TaskBar.SetEmpty();
        try
        {
            foreach (var process in ChildProcesses)
                try
                {
                    process.Kill();
                }
                catch
                {
                    // ignore
                }

            LogStreams.ScanBrutedLog.Flush();
            LogStreams.ScanBrutedLog.Close();

            LogStreams.AllPasswordsLog.Flush();
            LogStreams.AllPasswordsLog.Close();

            LogStreams.ErrorLog.Flush();
            LogStreams.ErrorLog.Close();

            LogStreams.ScanLog.Flush();
            LogStreams.ScanLog.Close();

            LogStreams.DisposeMnemo();

            FileSystem.RecursiveDelete(new DirectoryInfo(TempDir));
        }
        catch
        {
            // ignored
        }
    }

    #region VARIABLES

    internal static string Path = string.Empty;

    internal static List<string> DirsList = new();

    internal static Settings Settings = new();

    internal static string TempDir = string.Empty;

    internal static List<Process> ChildProcesses = new();

    internal static EventHandler ProcessExit;

    internal const bool DEBUG = false;

    internal static EnumeratorWrapper<Proxy> ProxyList;

    internal static int ProxyCount;

    #endregion
}
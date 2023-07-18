global using Console = Colorful.Console;
global using System.Drawing;
using CryptoEat.Modules;
using CryptoEat.Modules.Logs;
using CryptoEat.Modules.Scanners;
using Debank;

Helpers.SetTitle();
TaskBar.SetEmpty();

Generic.SetPriority();

Generic.GreetUser();

LogStreams.Initialize();
Generic.LoadSettings();
LogStreams.InitializeAfterSettings();
await Generic.CheckAntipublicAccess();

Console.WriteLine();
Generic.RequestPath();
Generic.ProcessPath();

Generic.SetOnClose();

DeBank.Migrate();
TronScan.LoadCache();

Logs.ProcessLogs();
Check.CheckLogs();

Generic.OnExit();
Console.WriteLine("[=] Check is completed!", Color.LightCoral);
AppDomain.CurrentDomain.ProcessExit -= Generic.ProcessExit;

Helpers.SetTitle();
Console.ReadLine();
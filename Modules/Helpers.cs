namespace CryptoEat.Modules;

internal static class Helpers
{
    private const string Tag = "2.0";
    internal static int Count;
    internal static int Max;

    internal static void SetTitle(string data)
    {
        var addiction = Count > 0 && Max > 0 ? $"[{Math.Round(GetPercent(Max, Count), 2)}%] " : "";
        if (addiction != "") TaskBar.SetProgress(GetPercent(Max, Count));

        var title = $"{addiction}CryptoEat {Tag} | {data}";

        if (Console.Title == title) return;
        Console.Title = title;
    }

    internal static void SetTitle()
    {
        Console.Title = $"CryptoEat {Tag}";
    }

    internal static void WriteTip(string tip)
    {
        Console.Write("[TIP] ", Color.LightCoral);
        Console.WriteLine(tip, Color.Coral);
    }

    internal static void ResetCounters()
    {
        Count = 0;
        Max = 0;
        TaskBar.SetEmpty();
    }

    internal static decimal GetPercent(int maxCount, int current, decimal percents = 100m)
    {
        return maxCount switch
        {
            0 => 0,
            _ => current / (maxCount / percents)
        };
    }
}
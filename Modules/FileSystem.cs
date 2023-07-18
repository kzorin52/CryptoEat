namespace CryptoEat.Modules;

internal class FileSystem
{
    internal static long DirsFound;
    internal static IEnumerable<string> ScanDirectories(string pathDir, bool title)
    {
        var spliter = DirsFound switch
        {
            > 1000 => 1000,
            > 100 => 100,
            > 10 => 10,
            _ => 1
        };
        if (DirsFound % spliter == 0 && title)
            Helpers.SetTitle($"Found [{DirsFound:N0}] folders, keep looking...");

        yield return pathDir;
        DirsFound++;

        IEnumerable<string> dirs;
        try
        {
            dirs = Directory.EnumerateDirectories(pathDir);
        }
        catch
        {
            dirs = Enumerable.Empty<string>();
        }

        foreach (var dirsLst in dirs.SelectMany(x => ScanDirectories(x, title)))
        {
            DirsFound++;
            yield return dirsLst;
        }
    }

    internal static void CopyDirectory(string oldPath, string newPath)
    {
        if (!Directory.Exists(newPath))
            Directory.CreateDirectory(newPath);

        var di = new DirectoryInfo(oldPath);

        var files = di.EnumerateFiles().ToList();
        foreach (var file in files)
            file.CopyTo(Path.Combine(newPath, file.Name));
    }

    internal static void RecursiveDelete(DirectoryInfo baseDir)
    {
        if (!baseDir.Exists)
            return;

        foreach (var dir in baseDir.EnumerateDirectories()) RecursiveDelete(dir);

        try
        {
            baseDir.Delete(true);
        }
        catch
        {
            // ignore
        }
    }
}
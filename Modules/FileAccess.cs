using System.Security.AccessControl;
using System.Security.Principal;

namespace CryptoEat.Modules;

public static class FileAccessHelper
{
    public static void CheckAndGrantAccess(string filePath)
    {
        try
        {
            // Check if file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            // Get file info
            var fileInfo = new FileInfo(filePath);

            // Check file permissions
            CheckDirectoryAndFilePermissions(fileInfo.Directory, fileInfo);

            // Check parent directory permissions
            var parentDirectory = fileInfo.Directory.Parent;
            while (parentDirectory != null)
            {
                CheckDirectoryPermissions(parentDirectory);
                parentDirectory = parentDirectory.Parent;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to grant access to file {filePath}: {ex.Message}");
            throw;
        }
    }

    private static void CheckDirectoryAndFilePermissions(DirectoryInfo directoryInfo, FileInfo fileInfo)
    {
        // Check directory permissions
        CheckDirectoryPermissions(directoryInfo);

        // Check file permissions
        CheckFilePermissions(fileInfo);
    }

    private static void CheckDirectoryPermissions(DirectoryInfo directoryInfo)
    {
        // Get directory security
        var security = directoryInfo.GetAccessControl();

        // Check access rules
        var rules = security.GetAccessRules(true, true, typeof(NTAccount));
        if (rules.Cast<FileSystemAccessRule>().Any(rule => (rule.FileSystemRights & FileSystemRights.FullControl) == FileSystemRights.FullControl))
        {
            return;
        }

        // Add full control to current user
        security.AddAccessRule(new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl,
            AccessControlType.Allow));

        // Apply changes to directory
        directoryInfo.SetAccessControl(security);
    }

    private static void CheckFilePermissions(FileInfo fileInfo)
    {
        // Get file security
        var security = fileInfo.GetAccessControl();

        // Check access rules
        var rules = security.GetAccessRules(true, true, typeof(NTAccount));
        if (rules.Cast<FileSystemAccessRule>().Any(rule => (rule.FileSystemRights & FileSystemRights.FullControl) == FileSystemRights.FullControl))
        {
            return;
        }

        // Add full control to current user
        security.AddAccessRule(new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl,
            AccessControlType.Allow));

        // Apply changes to file
        fileInfo.SetAccessControl(security);
    }
}
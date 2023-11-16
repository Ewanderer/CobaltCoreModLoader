using Microsoft.Win32;
using System.Runtime.Versioning;
using VdfParser;

namespace CobaltCoreModding.Components.Utils;

public static class FindGameFolder
{
    private const string SteamGameExe = "CobaltCore.exe";
    private const string SteamGameName = "Cobalt Core";
    private const string SteamInstallKeyName = "InstallPath";
    private const string SteamInstallSubKey32 = @"SOFTWARE\Valve\Steam";
    private const string SteamInstallSubKey64 = @"SOFTWARE\WOW6432Node\Valve\Steam";

    public static string FindGamePath()
    {
        if (OperatingSystem.IsWindows() && CheckWindowsRegistry(out var foundPath))
        {
            return foundPath;
        }

        return "";
    }

    [SupportedOSPlatform("windows")]
    private static bool CheckWindowsRegistry(out string foundPath)
    {
        foundPath = "";
        var installLocations = new List<string>(2);
        foreach (var subkey in new[] { SteamInstallSubKey32, SteamInstallSubKey64 })
        {
            using var key = Registry.LocalMachine.OpenSubKey(subkey);
            var value = key?.GetValue(SteamInstallKeyName, null);
            if (value is string sValue)
            {
                installLocations.Add(sValue);
            }
        }

        if (installLocations.Count == 0)
        {
            // Console.WriteLine("Failed to find steam install paths");
            return false;
        }

        var libraryLocations = new List<string>();
        foreach (var installLocation in installLocations)
        {
            var libraryVdfPath = Path.Combine(installLocation, "steamapps", "libraryfolders.vdf");
            using var libraryVdfFile = File.OpenRead(libraryVdfPath);
            var deserializer = new VdfDeserializer();
            if (deserializer.Deserialize(libraryVdfFile) is not IDictionary<string, dynamic> result)
            {
                // Console.WriteLine($"Failed to deserialize vdf file at '{libraryVdfPath}'");
                continue;
            }

            if (result["libraryfolders"] is not IDictionary<string, dynamic> libraryFoldersVdfEntry)
            {
                // Console.WriteLine($"No libraryfolders in vdf file at '{libraryVdfPath}'");
                continue;
            }

            foreach (var folderDynamic in libraryFoldersVdfEntry.Values)
            {
                if (folderDynamic is not IDictionary<string, dynamic> folderDict)
                {
                    // Console.WriteLine($"LibraryFolders is not a list of dict at '{libraryVdfPath}'");
                    continue;
                }

                if (folderDict["path"] is not string path)
                {
                    // Console.WriteLine($"Path is not a string in '{libraryVdfPath}'");
                    continue;
                }

                libraryLocations.Add(path);
            }
        }

        if (libraryLocations.Count == 0)
        {
            // Console.WriteLine($"Found {installLocations.Count} vdf files, but none contained any steam library paths");
            return false;
        }

        foreach (var libraryPath in libraryLocations)
        {
            var folderPath = Path.Combine(libraryPath, "steamapps", "common", SteamGameName);
            var exePath = Path.Combine(folderPath, SteamGameExe);
            if (!File.Exists(exePath)) continue;
            foundPath = folderPath;
            return true;
        }

        // Console.WriteLine("None of the steam library folders contain cobalt core");
        return false;
    }
}
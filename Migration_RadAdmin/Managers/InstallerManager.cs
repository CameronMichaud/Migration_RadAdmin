using System.Diagnostics;

using Migration_RadAdmin.Output;

namespace Migration_RadAdmin.Installers;

internal class InstallManager
{
    public static async Task GetInstaller(string url, string filePath)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fs);

        OutputManager.Log($"Download Complete: {filePath}");
    }

    public static bool DotNetInstalled(string version)
    {
        try
        {
            ProcessStartInfo funcInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--list-sdks",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = Process.Start(funcInfo);

            // Get list of SDKs
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Cut each line, see if it starts with the version requested, return true if so
            return output.Split('\n').Any(line => line.Trim().StartsWith(version));
        }
        catch
        {
            OutputManager.Log($".NET version {version} not found");
            return false;
        }
    }

    public static bool IsInstalled(string command, string args)
    {
        try
        {
            ProcessStartInfo funcInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = Process.Start(funcInfo);

            // If output is an error, return false
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return (!string.IsNullOrWhiteSpace(output) && !string.IsNullOrEmpty(output));
        }
        catch
        {
            return false;
        }
    }

    public static void GetServices()
    {
        try
        {
            // Grab al files in the current directory that has skyview-services*.msi
            string path = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo directory = new DirectoryInfo(path);
            FileInfo[] files = directory.GetFiles("skyview-services-*.msi");

            // Make a map of the services and their versions
            var servicesVersions = new List<(string filename, int[] version)>();
            foreach (var file in files)
            {
                int[] version = GetVersion(file.Name);
                servicesVersions.Add((file.Name, version));
            }

            // Iterate over the map, get the latest version
            var latest = servicesVersions[0];
            foreach (var file in servicesVersions)
            {
                if (CompareVersions(file.version, latest.version) > 0) // Returned True
                {
                    latest = file;
                }
            }

            // Install latest MSI
            OutputManager.Log($"Latest version: {latest.filename}");
            InstallServices($"{latest.filename}");
        }
        catch (Exception ex)
        {
            OutputManager.Log($"Services not found: {ex.Message}");
            MessageBox.Show($"Services not found\nPlease verify Skyview-Services-x.x.xxx.msi is located in the same directory as Migration_Admin.exe and try again", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    static int[] GetVersion(string filename)
    {
        // Assuming format: Skyview-Services-x.x.xxx.msi
        char v1 = filename[17];
        char v2 = filename[19];
        string v3 = filename.Substring(21, 3);

        // Convert to ints to compare
        int ver1 = int.Parse(v1.ToString());
        int ver2 = int.Parse(v2.ToString());
        int ver3 = int.Parse(v3);

        // Return int array of the versions
        return new int[] { ver1, ver2, ver3 };
    }

    static int CompareVersions(int[] v1, int[] v2)
    {
        // Compare version arrays, newer:1, older:-1, same:0
        for (int i = 0; i < 3; i++)
        {
            if (v1[i] > v2[i]) return 1;    // Version is newer
            if (v1[i] < v2[i]) return -1;   // Version is older
        }
        return 0; // Same version
    }

    public static bool InstallServices(string installFile)
    {
        // Directory of the exe
        string scriptDir = AppDomain.CurrentDomain.BaseDirectory;

        // Get the full path as string
        string migrationInstallPath = Path.Combine(scriptDir, installFile);

        // Run the process for installing the MSI as found by GetServices()
        if (File.Exists(migrationInstallPath))
        {
            InstallService(migrationInstallPath);

            return true;
        }
        else
        {
            OutputManager.Log($"No services found at: {migrationInstallPath}");

            return false;
        }
    }

    private static void InstallService(string path)
    {
        ProcessStartInfo funcInfo = new ProcessStartInfo
        {
            FileName = "msiexec",
            Arguments = $"/i \"{path}\" /passive",
            UseShellExecute = true
        };

        OutputManager.Log($"Installing Skyview services found at: {path}");
        var process = Process.Start(funcInfo);
        process?.WaitForExit();

        File.Delete(path); // Delete the installer after running it
    }
}


using Migration_RadAdmin.Installers;
using Migration_RadAdmin.Output;
using Migration_RadAdmin.Processes;
using Migration_RadAdmin.Users;
using System.Diagnostics;

namespace Migration_RadAdmin.Migration
{
    internal class MigrationManager
    {
        private static MainForm form;
        public static void Initialize(MainForm mainForm)
        {
            form = mainForm;
        }

        internal static async Task InstallDotNets()
        {
            string currentUser = Environment.UserName;

            await form.RunTask("Installing .NET SDKs", form.dotnetProgress, async () =>
            {
                bool dotnet6 = InstallManager.DotNetInstalled("6");
                if (!dotnet6)
                {
                    // DOWNLOAD
                    OutputManager.Log("Dowloading .NET 6 (this may take a few minutes)");
                    string URL = "https://builds.dotnet.microsoft.com/dotnet/Sdk/6.0.428/dotnet-sdk-6.0.428-win-x64.exe";
                    await InstallManager.GetInstaller(URL, $@"C:\users\{currentUser}\downloads\dotnet6.exe");
                    InstallDotNet(currentUser, "6");
                }
                else
                {
                    OutputManager.Log(".NET 6 already installed");
                }

                OutputManager.setProgress(form.dotnetProgress, 50);

                bool dotnet8 = InstallManager.DotNetInstalled("8");
                if (!dotnet8)
                {
                    // DOWNLOAD
                    OutputManager.Log("Dowloading .NET 8 (this may take a few minutes)");
                    string URL = "https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.100/dotnet-sdk-8.0.100-win-x64.exe";
                    await InstallManager.GetInstaller(URL, $@"C:\users\{currentUser}\downloads\dotnet8.exe");
                    InstallDotNet(currentUser, "8");
                }
                else
                {
                    OutputManager.Log(".NET 8 already installed");
                }
            });
        }

        internal static void InstallDotNet(string currentUser, string version)
        {
            OutputManager.setProgress(form.dotnetProgress, 75);

            // INSTALL
            OutputManager.Log($"Installing .NET {version} (this may take a few minutes)");
            ProcessManager.RunTerminal("cmd.exe", $@"/c C:\users\{currentUser}\downloads\dotnet{version}.exe /quiet");
            OutputManager.Log($".NET {version} install finished.\n");

            File.Delete($@"C:\\users\{currentUser}\downloads\dotnet{version}.exe");
        }

        internal static async Task UpdateUsers()
        {
            string currentUser = Environment.UserName;

            await form.RunTask("Updating Users", form.userProgress, async () =>
            {
                if (Environment.UserName.Equals("kiosk", StringComparison.OrdinalIgnoreCase))
                {
                    // Prevent deletion of the 'kiosk' user
                    OutputManager.Log("ERROR: Current user is 'kiosk'.");
                    MessageBox.Show($"Cannot update users.\nPlease run as Pf_Admin or Radianse", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    UserManager.LogoutUser("Kiosk");
                    UserManager.DeleteUser("Kiosk");
                    OutputManager.setProgress(form.userProgress, 50);
                    UserManager.RemoveUserPassword(currentUser);
                    OutputManager.setProgress(form.userProgress, 75);
                    UserManager.RenameUser(currentUser, "Radianse");
                }
            });
        }

        internal static async Task InstallChrome()
        {
            string currentUser = Environment.UserName;

            await form.RunTask("Installing Chrome", form.chromeProgress, async () =>
            {
                // Check if chrome exists or if there's a package manager, if so use them
                bool chrome = File.Exists(@"C:\Program Files\Google\Chrome\Application\chrome.exe");

                // If chrome does not exist, grab directly form URL
                if (!chrome)
                {
                    // If there's no package manager, download the file
                    string chromeInstaller = @"https://dl.google.com/chrome/install/latest/chrome_installer.exe";
                    string filePath = $@"C:\\users\{currentUser}\downloads\chrome_installer.exe";
                    await InstallManager.GetInstaller(chromeInstaller, filePath);

                    OutputManager.setProgress(form.chromeProgress, 75);

                    // Install Chrome
                    OutputManager.Log("Installing Chrome (this may take a few minutes)");
                    ProcessManager.RunTerminal("cmd.exe", $"/c \"{filePath} /silent /install\"");
                    File.Delete(filePath); // Delete the installer after running it
                }
                else
                {
                    OutputManager.Log("Chrome already installed");
                }
            });
        }

        internal static async Task InstallSkyview()
        {
            await form.RunTask("Installing Skyview Services", form.cleanProgress, async () =>
            {
                OutputManager.setProgress(form.cleanProgress, 50);

                // Install Skyview services
                InstallManager.GetServices();
            });
        }

        internal static void CompleteMigration()
        {
            OutputManager.setStatus("Migration Complete!");
            form.startButton.Text = "Complete";

            MessageBox.Show("Migration completed successfully.", "Migration Complete!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static async Task DeleteServiceCentral()
        {
            OutputManager.setStatus(@"Deleting Service Central (C:\ProgramData\Radianse)");
            await Task.Run(() => ProcessManager.DeleteServiceCentral());
        }

        internal static void SetStartup()
        {
            // INSTALL RADIANSE.IO AS APPLICATION:
            OutputManager.setStatus("Creating Radianse Shortcuts...");

            // Open radianse.io in Chrome, run shell:startup
            ProcessManager.ConfigureChrome();
        }
    }
}

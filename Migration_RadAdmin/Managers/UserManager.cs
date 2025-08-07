using System.DirectoryServices;

using Migration_RadAdmin.Output;
using Migration_RadAdmin.Processes;

namespace Migration_RadAdmin.Users
{
    internal class UserManager
    {
        public static void DeleteUser(string userName)
        {
            try
            {
                // Delete user
                ProcessManager.RunDelete("powershell.exe", $"Remove-LocalUser -Name \"{userName}\"");
                OutputManager.Log($"User '{userName}' deleted successfully.");
            }
            catch (Exception e)
            {
                OutputManager.Log($"Error deleting user: {e.Message}Likely user {userName} does not exist.");
            }
        }

        public static void RenameUser(string oldName, string newName)
        {
            try
            {
                // Check if user exists
                bool user = ProcessManager.RunReturn("powershell.exe", $"Get-LocalUser -Name {oldName}");

                if (user)
                {
                    // Rename user
                    ProcessManager.RunTerminal("powershell.exe", $"Set-LocalUser -Name {oldName} -FullName {newName}");
                    OutputManager.Log($"User '{oldName}' renamed successfully to '{newName}'.");
                }
                else
                {
                    OutputManager.Log($"User '{oldName}' not found.");
                }
            }
            catch (Exception e)
            {
                OutputManager.Log($"Error renaming user: {e.Message}");
            }
        }

        public static void RemoveUserPassword(string userName)
        {
            try
            {
                // Check if user exists
                DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName);
                DirectoryEntry user = localMachine.Children.Find(userName);

                if (user != null)
                {
                    // Remove password; this was the only way I could find to do it without a password
                    user.Invoke("SetPassword", new object[] { "" });
                    user.CommitChanges();
                    OutputManager.Log($"Password for '{userName}' was removed successfully.");
                }
                else
                {
                    OutputManager.Log($"User '{userName}' not found.");
                }
            }
            catch (Exception e)
            {
                OutputManager.Log($"Error setting password: {e.Message}");
            }
        }

        public static void LogoutUser(string userName)
        {
            try
            {
                ProcessManager.RunLogoff(userName);
            }
            catch (Exception e)
            {
                OutputManager.Log($"Error logging out user: {e.Message}");
            }
        }
    }
}

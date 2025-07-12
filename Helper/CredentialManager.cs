using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Live_Rate_Application.Helper
{
    public class CredentialManager
    {
        private static readonly string CredentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.dat");

        public CredentialManager(string username, string password, bool remember)
        {
            try
            {
                if (!remember)
                {
                    DeleteCredentials();
                    return;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(CredentialsPath));

                // Encrypt the password
                byte[] encryptedData = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes($"{username}|||{password}"),
                    null,
                    DataProtectionScope.CurrentUser);

                File.WriteAllBytes(CredentialsPath, encryptedData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save credentials: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static (string Username, string Password) LoadCredentials()
        {
            try
            {
                if (!File.Exists(CredentialsPath))
                    return (null, null);

                byte[] encryptedData = File.ReadAllBytes(CredentialsPath);
                byte[] decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
                string allData = Encoding.UTF8.GetString(decryptedData);

                string[] parts = allData.Split(new[] { "|||" }, StringSplitOptions.None);
                return parts.Length == 2 ? (parts[0], parts[1]) : (null, null);
            }
            catch
            {
                return (null, null);
            }
        }

        public static void DeleteCredentials()
        {
            try
            {
                if (File.Exists(CredentialsPath))
                    File.Delete(CredentialsPath);
            }
            catch { }
        }
    }
}

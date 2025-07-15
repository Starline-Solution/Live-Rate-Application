using IWshRuntimeLibrary;
using System;
using System.IO;

namespace Live_Rate_Application.Helper
{
    public class DesktopShortcut
    {
        public DesktopShortcut()
        {
            string excelFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Live Rate", "Live Rate.xlsx");
            string shortcutName = "Live Rate.lnk";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopPath, shortcutName);

            CreateExcelShortcut(excelFilePath, shortcutPath, "/x");
        }

        public void CreateExcelShortcut(string excelFilePath, string shortcutPath, string argument)
        {
            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = GetExcelPath();
            shortcut.Arguments = $"{argument} \"{excelFilePath}\"";
            shortcut.Description = "Open Live Rate With Admin Privilege";
            shortcut.IconLocation = $"{GetExcelPath()}, 0";
            
            // Save the shortcut first
            shortcut.Save();

            // Modify the shortcut to run as administrator
            ModifyShortcutToRunAsAdmin(shortcutPath);
        }

        public string GetExcelPath()
        {
            // Try common Excel paths first
            string[] commonPaths = {
                @"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE",
                @"C:\Program Files\Microsoft Office\Office16\EXCEL.EXE",
                @"C:\Program Files (x86)\Microsoft Office\root\Office16\EXCEL.EXE"
            };

            foreach (var path in commonPaths)
            {
                if (System.IO.File.Exists(path)) return path;
            }

            // Fallback to registry lookup
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\excel.exe"))
            {
                return key?.GetValue("")?.ToString() ?? "EXCEL.EXE";
            }
        }

        private void ModifyShortcutToRunAsAdmin(string shortcutPath)
        {
            // Read all bytes of the shortcut file
            byte[] fileBytes = System.IO.File.ReadAllBytes(shortcutPath);

            // The flag for "Run as administrator" is at position 21 (0x15)
            // 0x22 is the value that enables "Run as administrator"
            if (fileBytes.Length > 0x15)
            {
                fileBytes[0x15] |= 0x22;
                System.IO.File.WriteAllBytes(shortcutPath, fileBytes);
            }
        }
    }
}
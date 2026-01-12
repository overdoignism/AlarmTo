using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlarmTo
{

    internal static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BootForm());
        }
    }

    public class StartupManager
    {
        private const string RUN_KEY_PATH = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        /// <summary>
        /// 產生 AppName + SHA256(appPath 前 8 hex) 的唯一名稱
        /// </summary>
        private static string GetUniqueAppName()
        {
            string appName = Application.ProductName;
            string appPath = Application.ExecutablePath;

            // 計算 SHA256
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(appPath));
                // 轉 hex，取前 8 字元
                string hex8 = BitConverter.ToString(hash).Replace("-", "").Substring(0, 8);
                return $"{appName}_{hex8}";
            }
        }

        public static void EnableStartup()
        {
            RegistryKey rk = null;
            try
            {
                string uniqueName = GetUniqueAppName();
                string appPath = Application.ExecutablePath;

                rk = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH, writable: true);

                if (rk.GetValue(uniqueName) == null)
                {
                    rk.SetValue(uniqueName, appPath);
                }

                MessageBox.Show("\"Start with Windows\" enabled.", "Message", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failure: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                rk?.Close();
            }
        }

        public static void DisableStartup()
        {
            RegistryKey rk = null;
            try
            {
                string uniqueName = GetUniqueAppName();

                rk = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH, writable: true);

                if (rk.GetValue(uniqueName) != null)
                {
                    rk.DeleteValue(uniqueName);
                }

                MessageBox.Show("\"Start with Windows\" disabled.", "Message", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failure: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                rk?.Close();
            }
        }

        public static void CleanupStartupEntries()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH, writable: true);

                if (rk == null)
                {
                    MessageBox.Show("Cannot open the startup registry key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 1. Find all keys starting with AlarmTo_
                var valueNames = rk.GetValueNames()
                                   .Where(name => name.StartsWith("AlarmTo_", StringComparison.OrdinalIgnoreCase))
                                   .ToList();


                // 2. Find keys with existing paths
                var NotexistingList = new List<string>();
                foreach (string name in valueNames)
                {
                    string path = rk.GetValue(name)?.ToString();
                    if (path == null || !File.Exists(path))
                    {
                        NotexistingList.Add(name);
                    }
                }

                // 3. If any existing entries, ask user whether to clean up
                if (NotexistingList.Count > 0)
                {
                    string msg = "Found the invalid AlarmTo startup entries :\n\n" +
                                 string.Join("\n", NotexistingList) +
                                 "\n\nDo you want to clean up?";

                    var result = MessageBox.Show(msg, "Cleanup Startup Entries", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result != DialogResult.Yes)
                    {
                        //MessageBox.Show("Operation canceled.", "Message", MessageBoxButtons.OK);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("No invalid startup entries found.", "Message", MessageBoxButtons.OK);
                    return;
                }

                // 4. Delete keys with missing paths
                foreach (string name in NotexistingList)
                {
                    rk.DeleteValue(name, false);
                }

                // 5. Show completion message
                MessageBox.Show("Clean completed.", "Message", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failure: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                rk?.Close();
            }
        }
    }

}


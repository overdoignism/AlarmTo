using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlarmTo
{
    public partial class FormHelp : Form
    {
        public FormHelp()
        {
            InitializeComponent();
        }

        private void CloseBTN_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void FormHelp_Load(object sender, EventArgs e)
        {
            HelpTxt.DeselectAll();
        }

        private void BTN_tips_Click(object sender, EventArgs e)
        {
            LicenseTxt.Visible = false;
            HelpTxt.Visible = true; 
        }

        private void BTN_Lic_Click(object sender, EventArgs e)
        {
            LicenseTxt.Visible = true;
            HelpTxt.Visible = false;
        }

        private void GoGitBTN_Click(object sender, EventArgs e)
        {
            string targetUrl = "https://github.com/overdoignism/AlarmTo";
            BrowserLauncher.OpenUrl(targetUrl);
        }
    }

    public class BrowserLauncher
    {
        public static void OpenUrl(string url)
        {
            try
            {
                // **推薦使用:** 這是跨平台且最安全的開啟網址方法
                // 對於 .NET Core / .NET 5+，使用 ProcessStartInfo.UseShellExecute = true 是標準做法。
                // 對於 .NET Framework，它通常是預設值。

                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // 處理在特定系統上可能發生的異常，例如找不到關聯的應用程式。

                // 由於 Process.Start 在不同作業系統上的行為差異，
                // 這裡提供一個針對舊版/特殊環境的備用方案 (尤其是 Linux/macOS)。

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows 專用 (例如在 .NET Framework 環境下)
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Linux 專用
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS 專用
                    Process.Start("open", url);
                }
                else
                {
                    // 如果所有方法都失敗，可以輸出錯誤訊息
                    Console.WriteLine($"無法開啟網址 {url}. 錯誤: {ex.Message}");
                }
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlarmTo
{
    public partial class BootForm : Form
    {

        private string thePID;
        private string theTitle;
        private bool isOneTimeInstance = false;

        //private Form mainAlarmForm;
        private MainForm mainAlarmForm;
        private FormHelp theFormHelp;

        public BootForm()
        {
            InitializeComponent();
            this.Width = 0;
            this.Height= 0;
        }

        private void Form2_Load(object sender, EventArgs e)
        {

            thePID = GetFormattedHexPid();
            mainAlarmForm = new MainForm(this);
            theFormHelp = new FormHelp();

            string[] argv = Environment.GetCommandLineArgs(); // argv[0] = 程式路徑

            for (int i = 1; i < argv.Length; i++)
            {
                string token = argv[i].TrimStart('/', '-');
                if (string.Equals(token, "Instance", StringComparison.OrdinalIgnoreCase))
                {
                    isOneTimeInstance = true;
                    break;
                }
            }

            string tooltipStr = "\nDouble-Click→Main\nRight-Click→Menu";

            if (isOneTimeInstance)
            {
                notifyIcon1.Icon = Properties.Resources.BELL_Inst;
                thePID = "Instance-" + thePID;
                notifyIcon1.Text = "AlarmTo [" + thePID.ToString() + "]" + tooltipStr;
            }
            else
            {
                if (Properties.Settings.Default.AlarmTitle == "" || Properties.Settings.Default.AlarmTitle == null)
                {
                    notifyIcon1.Text = "AlarmTo [" + thePID.ToString() + "]" + tooltipStr;
                }
                else
                {
                    theTitle = Properties.Settings.Default.AlarmTitle;
                    notifyIcon1.Text = "AlarmTo [" + theTitle + "]" + tooltipStr;
                }
            }

            mainAlarmForm.isOneTimeInstance = isOneTimeInstance;
            mainAlarmForm.thePID = thePID;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowMain();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            // 點擊左鍵時，手動顯示選單
            // 必須知道滑鼠位置才能顯示
            //ContextMenuStrip1.Show(Control.MousePosition);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void showMenuItem_Click(object sender, EventArgs e)
        {
            ShowMain();
        }

        private void ShowMain() 
        {
            mainAlarmForm.Show();
            mainAlarmForm.Activate();
            mainAlarmForm.LastWindowIsVisible = true;
        }

        public static string GetFormattedHexPid()
        {
            // 1. 獲取當前行程 ID (PID)
            int currentProcessId = Process.GetCurrentProcess().Id;

            // 2. 將 PID 轉換為大寫 Hex 字串 ("X")
            // "X" 標準格式字串表示十六進制大寫。
            string hexString = currentProcessId.ToString("X");

            // 3. 補足 '0' 至至少 4 位長度
            // PadLeft(4, '0') 確保字串長度至少為 4。
            // 如果 hexString 已經是 5 位或更長 (例如 "1B34A")，則不會被填充。
            return hexString.PadLeft(4, '0');
        }

        public static void RelaunchWithInstanceArg()
        {
            try
            {
                // 取得自己的執行檔路徑
                string exePath = Application.ExecutablePath;

                // 準備啟動參數
                string arguments = "/Instance";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = false
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to relaunch: " + ex.Message);
            }
        }

        private void runInstMenuItem_Click(object sender, EventArgs e)
        {
            RelaunchWithInstanceArg();
        }

        private void helpMenuItem_Click(object sender, EventArgs e)
        {
            theFormHelp.Show();
        }
    }
}

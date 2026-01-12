using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
//using NAudio.Utils; // 確保有這個，雖然 VolumeSampleProvider 位於 NAudio.Wave
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AlarmTo
{

    public partial class MainForm : Form
    {
        private FormHelp theFormHelp;
        private BootForm _parentForm;

        private AudioDevice selectedDevice;
        private int the_Hours = 0;
        private int the_Minutes = 0;
        private int the_Mode = 0;
        private bool AlarmOnOff_bool = false;
        private bool[] week_on = new bool[7]; 
        private readonly Random random = new Random();

        //private IWavePlayer waveOut;
        //private AudioFileReader audioFile;
        //private VolumeSampleProvider volumeProvider;        // 新增：音量控制器

        public string thePID;
        public string theAlarmTitle;
        public int AlarmIconNum = 0;
        public bool isOneTimeInstance;
        public bool LastWindowIsVisible = false;


        public MainForm(BootForm parentForm)
        {
            InitializeComponent();
            AlarmOnOff.Text = "ALARM\nOFF";
            _parentForm = parentForm;
            PopulateAudioDevices();
            LoadSettings();
            AlarmOnOff_Show();
            this.Width = 630;
            this.Height = 530;
            RefresAppTitle("* Settings*");
            snoozeBTN.Text = "Snooze\n" + soonzeNum.Value.ToString() + " min";

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupComboBox(FileListComboBox,0);
            SetupComboBox(ADCBox,1);
            InstanceWork();
            theFormHelp = new FormHelp();
        }
        private void HandlePlaybackStopped(object sender, StoppedEventArgs e)
        {
            TestPlayBtnReset();
        }

        private void RefresAppTitle(string WhatToAppend)
        {
            if ((theAlarmTitle == "") || (theAlarmTitle == null))
            {
                this.Text = "AlarmTo [" + thePID + "] " + WhatToAppend;
            }
            else
            {
                this.Text = "AlarmTo [" + theAlarmTitle + "] " + WhatToAppend; 
            }
        }

        public class ComboBoxItem
        {
            public string Name { get; set; }
            public string DeviceId { get; set; } // 這是我們播放時真正需要的

            public override string ToString()
            {
                return Name; // ComboBox 顯示名稱
            }
        }

        private void PopulateAudioDevices()
        {
            // 1. 清空舊的列表
            ADCBox.Items.Clear();

            // 2. 創建 MMDeviceEnumerator 實例 (NAudio 抽象了 WASAPI COM)
            using (var enumerator = new MMDeviceEnumerator())
            {
                // 3. 列舉所有處於「啟用 (Active)」狀態的「播放 (Render)」裝置
                var devices = enumerator.EnumerateAudioEndPoints(
                    DataFlow.Render,
                    DeviceState.Active
                );

                // 4. 檢查是否有裝置
                if (devices.Count == 0)
                {
                    MessageBox.Show("No audio device was found.\n\nProgram terminated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);                    
                }

                // 5. 循環並將裝置添加到 ComboBox
                foreach (var device in devices)
                {
                    // 將裝置友好名稱和 ID 儲存到我們的自定義類別中
                    var audioDevice = new AudioDevice
                    {
                        FriendlyName = device.FriendlyName,
                        ID = device.ID
                    };

                    // 將物件添加到 ComboBox
                    ADCBox.Items.Add(audioDevice);
                }

                // 6. 預設選中第一個裝置
                if (ADCBox.Items.Count > 0)
                {
                    ADCBox.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// 儲存當前選定的音訊裝置資訊。
        /// </summary>
        private void SaveSettings()
        {
            var selectedDevice = (AudioDevice)ADCBox.SelectedItem;         
            Properties.Settings.Default.LastDeviceID = selectedDevice.ID;    // 1. 儲存唯一 ID
            Properties.Settings.Default.LastDeviceName = selectedDevice.FriendlyName;
            Properties.Settings.Default.AlarmIsSet = AlarmOnOff_bool;
            Properties.Settings.Default.AlarmOnW0 = WeekAlarm0.Checked;
            Properties.Settings.Default.AlarmOnW1 = WeekAlarm1.Checked;
            Properties.Settings.Default.AlarmOnW2 = WeekAlarm2.Checked;
            Properties.Settings.Default.AlarmOnW3 = WeekAlarm3.Checked;
            Properties.Settings.Default.AlarmOnW4 = WeekAlarm4.Checked;
            Properties.Settings.Default.AlarmOnW5 = WeekAlarm5.Checked;
            Properties.Settings.Default.AlarmOnW6 = WeekAlarm6.Checked;
            Properties.Settings.Default.AlarmOnHour = the_Hours;
            Properties.Settings.Default.AlarmOnMinute = the_Minutes;
            Properties.Settings.Default.PlayFile = FileListComboBox.Text;// FileHereTextbox.Text;
            Properties.Settings.Default.PlayFile = ExportComboBoxItems(FileListComboBox, "?");

            Properties.Settings.Default.UseLoop = LoopCheck.Checked;
            Properties.Settings.Default.Topmost = TMCheck.Checked;
            Properties.Settings.Default.AlarmMsg = AlarmMsgTxtWrite.Text;
            Properties.Settings.Default.AlarmTitle = AlarmTitleTxt.Text;
            Properties.Settings.Default.soonzeMin = soonzeNum.Value;
            Properties.Settings.Default.AlarmIconNum = AlarmIconNum;

            Properties.Settings.Default.Save();             // 2. 提交變更到設定檔案

            MessageBox.Show("Settings saved.", "Message", MessageBoxButtons.OK );
        }

        /// <summary>
        /// 載入並選中上次儲存的音訊裝置。如果找不到，則清除設定並選中第一個項目。
        /// </summary>
        private void LoadSettings()
        {
            
            // 取得上次儲存的裝置 ID
            string lastID = Properties.Settings.Default.LastDeviceID;
            bool deviceFound = false;

            AlarmOnOff_bool = Properties.Settings.Default.AlarmIsSet;
            WeekAlarm0.Checked = Properties.Settings.Default.AlarmOnW0;
            week_on[0] = Properties.Settings.Default.AlarmOnW0;
            WeekAlarm1.Checked = Properties.Settings.Default.AlarmOnW1;
            week_on[1] = Properties.Settings.Default.AlarmOnW1;
            WeekAlarm2.Checked = Properties.Settings.Default.AlarmOnW2;
            week_on[2] = Properties.Settings.Default.AlarmOnW2;
            WeekAlarm3.Checked = Properties.Settings.Default.AlarmOnW3;
            week_on[3] = Properties.Settings.Default.AlarmOnW3;
            WeekAlarm4.Checked = Properties.Settings.Default.AlarmOnW4;
            week_on[4] = Properties.Settings.Default.AlarmOnW4;
            WeekAlarm5.Checked = Properties.Settings.Default.AlarmOnW5;
            week_on[5] = Properties.Settings.Default.AlarmOnW5;
            WeekAlarm6.Checked = Properties.Settings.Default.AlarmOnW6;
            week_on[6] = Properties.Settings.Default.AlarmOnW6;
            the_Hours = Properties.Settings.Default.AlarmOnHour;
            the_Minutes = Properties.Settings.Default.AlarmOnMinute;
            LoopCheck.Checked = Properties.Settings.Default.UseLoop;
            TMCheck.Checked = Properties.Settings.Default.Topmost;
            soonzeNum.Value = Properties.Settings.Default.soonzeMin;

            if (!isOneTimeInstance) { 
                AlarmIconNum = Properties.Settings.Default.AlarmIconNum;
                iconTrack.Value = AlarmIconNum;
            }
            SelectIcon(AlarmIconNum);

            if (!(Properties.Settings.Default.PlayFile == "")) 
            {
                ImportComboBoxItems(FileListComboBox, Properties.Settings.Default.PlayFile, "?");
            }
            else
            {
                FileListComboBox.SelectedIndex = 0;
            }

            if (!(Properties.Settings.Default.AlarmMsg == ""))
            {
                AlarmMsgTxtWrite.Text = Properties.Settings.Default.AlarmMsg; 
            }

            the_Mode = 4;
            Refresh_Display(0);

            // 如果沒有儲存過 ID，則維持預設選中第一個項目並返回
            if (string.IsNullOrEmpty(lastID))
            {
                // 確保至少選中第一個項目，以防萬一
                if (ADCBox.Items.Count > 0 && ADCBox.SelectedIndex == -1)
                {
                    ADCBox.SelectedIndex = 0;
                }
                return;
            }

            // 循環遍歷 ComboBox 中的所有項目，嘗試匹配 ID
            for (int i = 0; i < ADCBox.Items.Count; i++)
            {
                var device = (AudioDevice)ADCBox.Items[i];

                // 匹配儲存的 ID
                if (device.ID == lastID)
                {
                    ADCBox.SelectedIndex = i;
                    deviceFound = true;
                    break;
                }
            }
           
            if (!deviceFound)  // --- 健壯性邏輯：處理裝置被移除的情況 ---
            {
                ADCBox.SelectedIndex = 0;

                MessageBox.Show($"The saved audio device: \n\n(" + Properties.Settings.Default.LastDeviceName + 
                    ") \n\nhas been removed.\n\n" + $"The first device is selected automatically.", 
                    "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InstanceWork()
        {
            if (!isOneTimeInstance)
            {
                if (!(Properties.Settings.Default.AlarmTitle == ""))
                {
                    AlarmTitleTxt.Text = Properties.Settings.Default.AlarmTitle;
                }
                else
                {
                    AlarmTitleTxt.Text = thePID;
                }
            }
            else
            {
                AlarmTitleTxt.Text = thePID;
                SwW_dis_BTN.Enabled = false;
                SwW_dis_BTN.BackColor = Color.Gray;
                SwW_en_BTN.Enabled = false;
                SwW_en_BTN.BackColor = Color.Gray;
                SwW_cln_BTN.Enabled = false;
                SwW_cln_BTN.BackColor = Color.Gray;
                SaveButton.Enabled = false;
                SaveButton.BackColor = Color.Gray;
                AlarmTitleTxt.Enabled = false;
                iconTrack.Enabled = false;
                AlarmIconNum = 0;
            }
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void AlarmOffScreen(bool GoShowSet, bool offAlarm)
        {
            if (!GoShowSet)
            {
                this.Hide();
            }

            if (offAlarm)
            {
                AlarmOnOff_bool = false;
                AlarmOnOff_Show();
            }

            selectedDevice.StopAlarmSound();

            PanelAlarm.Enabled = false;
            PanelAlarm.Visible = false;
            PanelSet.Enabled = true;
            PanelSet.Visible = true;
            AlarmShowIng = false;
            //AlarmStage = 1;

            this.TopMost = false;
            RefresAppTitle("* Settings*");
            this.BringToFront();
        }

        private void PlayAlarm(bool isLoop, bool isTest)
        {
            if (ADCBox.SelectedItem == null)
            {
                MessageBox.Show("Need to select an audio device.", "Hint");
                return;
            }

            // 2. 取得選定的 AudioDevice 物件
            selectedDevice = (AudioDevice)ADCBox.SelectedItem;
            selectedDevice.currentDevice = (AudioDevice)ADCBox.SelectedItem;

            selectedDevice.StopAlarmSound();  

            if (selectedDevice.IsAlarmPlaying() == false)
            {

                string playFilePath;

                if (isTest)
                {
                    playFilePath = FileListComboBox.Text;
                }
                else
                {
                    playFilePath = GetRandomExistingFilePath(FileListComboBox);
                }

                if (playFilePath == null)
                {
                    MessageBox.Show("All sound files are missing.", "error");
                    return;
                }

                selectedDevice.PlaybackStoppedEvent += HandlePlaybackStopped;
                selectedDevice.PlayAlarmSound(playFilePath, selectedDevice, isLoop); // 4. 執行播放
            }

        }

        private void Refresh_Display(int TimerInt)
        {

            switch(the_Mode)
            {
                case 0:
                    the_Hours++;
                    if (the_Hours >= 24)
                    {
                        the_Hours = 0;
                    }
                    break;
                case 1:
                    the_Hours--;
                    if (the_Hours <= 0)
                    {
                        the_Hours = 23;
                    }
                    break;
                case 2:
                    the_Minutes++;
                    if (the_Minutes >= 60)
                    {
                        the_Minutes = 0;
                    }
                    break;
                case 3:
                    the_Minutes--;
                    if (the_Minutes <= 0)
                    {
                        the_Minutes = 59;
                    }
                    break;
            }

            theDisplayH.Text = $"{the_Hours:D2}";
            theDisplayM.Text = $"{the_Minutes:D2}";

            if (the_Mode <= 3)
            {
                ButtonTimer.Interval = TimerInt;
                ButtonTimer.Enabled = true;
            }

        }
        private void ButtonTimer1_Tick(object sender, EventArgs e)
        {
            Refresh_Display(250);
        }

        private void HourUp_MouseDown(object sender, MouseEventArgs e)
        {
            the_Mode = 0;
            Refresh_Display(750);
            AlarmStage = 0;
        }

        private void HourDown_MouseDown(object sender, MouseEventArgs e)
        {
            the_Mode = 1;
            Refresh_Display(750);
            AlarmStage = 0;
        }

        private void MinuteUp_MouseDown(object sender, MouseEventArgs e)
        {
            the_Mode = 2;
            Refresh_Display(750);
            AlarmStage = 0;
        }

        private void MinuteDown_MouseDown(object sender, MouseEventArgs e)
        {
            the_Mode = 3;
            Refresh_Display(750);
            AlarmStage = 0;
        }

        private void All_MouseUp(object sender, MouseEventArgs e)
        {
            ButtonTimer.Enabled = false;
        }
        private void AlarmOnOff_Click(object sender, EventArgs e)
        {
            AlarmOnOff_bool = !AlarmOnOff_bool;
            AlarmOnOff_Show();
            if (AlarmOnOff_bool) 
            {
                AlarmStage = 0;
            }           
        }

        private void AlarmOnOff_Show()
        {
            if (AlarmOnOff_bool)
            {
                AlarmOnOff.BackColor = Color.FromArgb(255, 96, 96);
                AlarmOnOff.ForeColor = Color.White;
                AlarmOnOff.Text = "ALARM\nON";
                BootForm.Default.RefreshIcon(AlarmIconNum, true);
            }
            else
            {
                AlarmOnOff.BackColor = Color.FromArgb(224, 224, 224);
                AlarmOnOff.ForeColor = Color.Black;
                AlarmOnOff.Text = "ALARM\nOFF";
                BootForm.Default.RefreshIcon(AlarmIconNum, false);
            }            
        }

        private void Browsebutton_Click(object sender, EventArgs e)
        {

            // 1. 創建 OpenFileDialog 實例
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                // 設置過濾條件，例如只顯示音訊檔案
                ofd.Filter = "Audio file (WAV/MP3/M4A/AAC/FLAC)|*.wav;*.mp3;*.m4a;*.aac;*.flac";
                ofd.Title = "The alarm audio file";

                // 2. 顯示對話框並檢查結果
                // 只有當使用者點擊「開啟」時，結果才會是 DialogResult.OK
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    // 如果使用者點擊「取消」或關閉對話框，則直接退出函式
                    return;
                }

                // --- 3. 只有當檔案成功開啟時，才會執行到這裡 ---
                FileListComboBox.Items.Add(ofd.FileName);
                FileListComboBox.SelectedIndex = FileListComboBox.Items.Count - 1;
                // 接下來您可以處理檔案路徑，例如：
            }
        }

        private int AlarmStage = 0;
        private bool AlarmShowIng = false;
        private DateTime now;
        private void AlarmTimer_Tick(object sender, EventArgs e)
        {

            if (AlarmOnOff_bool == false)
            { return; }

            now = DateTime.Now;  // 取得當前的日期和時間

            if (AlarmStage == 0)
            {
                if (the_Minutes == now.Minute && the_Hours == now.Hour && week_on[(int)now.DayOfWeek] == true)
                {                            
                        AlarmStage = 1;
                        AlarmProcess();
                }
            }
            else if (AlarmStage >= 1)
            {

                if (AlarmStage == 1)
                {
                    if (the_Minutes != now.Minute || the_Hours != now.Hour || week_on[(int)now.DayOfWeek] == false)
                    {
                        AlarmStage = 0;
                    }
                }
                else if (AlarmStage == 2)
                {
                    if (snoozeCount >= snoozeMax)
                    {
                        AlarmStage = 3;
                        AlarmProcess();
                    }
                    else
                    {
                        snoozeCount++;
                    }
                }

            }

            if (AlarmShowIng)
            {
                NowTimeTxt.Text = now.ToString("HH:mm:ss");
                NowWeekTxt.Text = now.ToString("dddd");
            }

        }
        private void AlarmProcess()
        {
            AlarmShowTxt.Text = AlarmMsgTxtWrite.Text;
            PanelAlarm.Enabled = true;
            PanelAlarm.Visible = true;
            AlarmShowIng = true;
            PanelSet.Enabled = false;
            PanelSet.Visible = false;
            ButtonTimer.Enabled = false;
            NowTime_Tick(null, null);

            if (TMCheck.Checked)
            {
                this.TopMost = true;
                RefresAppTitle("*ALARM!*  (Topmost)");
            }
            else
            {
                this.Text = "AlarmTo [" + thePID + "]  *ALARM!*";
                RefresAppTitle("*ALARM!*");
            }

            this.Show();
            CenterWindowOnScreen();

            PlayAlarm(LoopCheck.Checked, false);
        }

        private void WeekAlarm0_CheckedChanged(object sender, EventArgs e){week_on[0] = WeekAlarm0.Checked;}
        private void WeekAlarm1_CheckedChanged(object sender, EventArgs e){week_on[1] = WeekAlarm1.Checked;}
        private void WeekAlarm2_CheckedChanged(object sender, EventArgs e){week_on[2] = WeekAlarm2.Checked;}
        private void WeekAlarm3_CheckedChanged(object sender, EventArgs e){week_on[3] = WeekAlarm3.Checked;}
        private void WeekAlarm4_CheckedChanged(object sender, EventArgs e){week_on[4] = WeekAlarm4.Checked;}
        private void WeekAlarm5_CheckedChanged(object sender, EventArgs e){week_on[5] = WeekAlarm5.Checked;}
        private void WeekAlarm6_CheckedChanged(object sender, EventArgs e){week_on[6] = WeekAlarm6.Checked;}

        private void NowTime_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            NowTimeTxt.Text = now.ToString("HH:mm:ss");
            NowWeekTxt.Text = now.ToString("dddd");
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void StopAlarmBTN_Click(object sender, EventArgs e)
        {
            AlarmOffScreen(LastWindowIsVisible, false);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 如果使用者不是強制退出 (例如使用 Alt+F4)，我們攔截關閉操作
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // 1. 取消關閉操作
                e.Cancel = true;

                // 2. 執行縮小到通知區的邏輯
                this.Hide();

                // 3. (可選) 給出一個氣泡提示
                //notifyIcon1.ShowBalloonTip(1000, "AlarmTo", "Doube-click to restore.", ToolTipIcon.Info);
            }
            // 如果 e.CloseReason 是 ApplicationExitCall (例如呼叫 Application.Exit())，則程式會正常退出。
        }

        private void MinBtn_Click(object sender, EventArgs e)
        {
            this.Hide();
            LastWindowIsVisible = false;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_SHOW = 5;

        public void CenterWindowOnScreen()
        {
           
            Screen currentScreen = Screen.FromControl(this);                        // 1. 獲取表單當前所在的螢幕 (如果使用多螢幕，這是關鍵
            Rectangle workingArea = currentScreen.WorkingArea;                      // 2. 獲取工作區的寬度和高度 (排除了任務欄的區域)

            // Rectangle bounds = currentScreen.Bounds; // 或者獲取整個螢幕的邊界 (包含任務欄，較少用)

            int newX = workingArea.X + (workingArea.Width - this.Width) / 2;        // 3. 計算新的 X 座標： (工作區寬度 - 表單寬度) / 
            int newY = workingArea.Y + (workingArea.Height - this.Height) / 2;      // 4. 計算新的 Y 座標： (工作區高度 - 表單高度) / 
            
            this.Location = new Point(newX, newY);                                  // 5. 設置表單的 Location 屬性

            ShowWindow(this.Handle, SW_SHOW);
            SetForegroundWindow(this.Handle);

        }

        private void StopAlarmExitBTN_Click(object sender, EventArgs e)
        {
            AlarmOffScreen(LastWindowIsVisible, false);
            Application.Exit();
        }

        /// <summary>
        /// 將 ComboBox 中的所有項目串接成一個字串，由指定的分隔符分隔。
        /// </summary>
        /// <param name="comboBox">要處理的 ComboBox 控制項。</param>
        /// <param name="separator">用於分隔項目的字串，例如 "?"。</param>
        /// <returns>包含所有項目的單一字串。</returns>
        public string ExportComboBoxItems(System.Windows.Forms.ComboBox comboBox, string separator)
        {
            // 如果 ComboBox 中沒有項目，則返回空字串
            if (comboBox.Items.Count == 0)
            {
                return string.Empty;
            }

            // 使用 LINQ 或 StringBuilder 來高效地串接字串
            // LINQ 版本：
            string combinedText = string.Join(separator, comboBox.Items.Cast<object>());

            // 由於 ComboBox.Items 包含的是 object 類型，Cast<object>() 後，
            // string.Join 會自動呼叫每個項目的 ToString()。

            return combinedText;
        }

        /// <summary>
        /// 清除 ComboBox 中的現有項目，並從給定的字串中還原新的項目。
        /// </summary>
        /// <param name="comboBox">要填充的 ComboBox 控制項。</param>
        /// <param name="combinedText">包含由分隔符分隔的項目的單一字串。</param>
        /// <param name="separator">用於分隔項目的字串，例如 "?"。</param>
        public void ImportComboBoxItems(System.Windows.Forms.ComboBox comboBox, string combinedText, string separator)
        {
            // 檢查輸入字串是否為 null 或空
            if (string.IsNullOrEmpty(combinedText))
            {
                comboBox.Items.Clear();
                return;
            }

            // 1. 清除所有現有項目
            comboBox.Items.Clear();

            // 2. 使用 String.Split 方法拆分字串
            // StringSplitOptions.RemoveEmptyEntries 確保不會添加空字串項目（以防分隔符連續出現）
            string[] itemsArray = combinedText.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            // 3. 將數組中的所有項目添加到 ComboBox
            comboBox.Items.AddRange(itemsArray);

            // 可選：選擇第一個項目
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private void MinusButton_Click(object sender, EventArgs e)
        {
            // 1. Check if an item is currently selected
            if (FileListComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an item.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Confirmation prompt (Yes/No)
            DialogResult result = MessageBox.Show(
                "Remove selected item?", // Simple confirmation question
                "Confirm Removal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                // Get the index of the item to be removed
                int removedIndex = FileListComboBox.SelectedIndex;

                // Execute the removal
                FileListComboBox.Items.RemoveAt(removedIndex);

                // 4. Update the selection after removal
                if (FileListComboBox.Items.Count > 0)
                {
                    // Try to select the item at the same index, 
                    // otherwise select the new last item if the index is out of bounds.
                    int newIndex = Math.Min(removedIndex, FileListComboBox.Items.Count - 1);
                    FileListComboBox.SelectedIndex = newIndex;
                }
                else
                {
                    // If the list is now empty, set to no selection
                    //FileListComboBox.SelectedIndex = -1;
                    FileListComboBox.Items.Add("C:\\Windows\\Media\\Windows Ding.wav");
                    FileListComboBox.SelectedIndex = 0;
                    MessageBox.Show("The list has been cleared. Using default file.", "Hint");
                }
            }
            // If result is No, do nothing and exit.
        }

        /// <summary>
        /// 隨機打散 ComboBox 項目，然後按順序檢查是否存在於檔案系統中。
        /// </summary>
        /// <param name="fileListBox">包含檔案路徑的 ComboBox 控制項。</param>
        /// <returns>第一個被找到的現有檔案路徑，如果所有路徑都不存在則返回 null。</returns>
        public string GetRandomExistingFilePath(System.Windows.Forms.ComboBox fileListBox)
        {
            // 1. 將 ComboBox 項目複製到一個字串陣列
            // ComboBox.Items 是 object 集合，需轉換為 string
            string[] paths = fileListBox.Items.Cast<object>()
                                             .Select(item => item?.ToString() ?? string.Empty)
                                             .ToArray();

            if (paths.Length == 0)
            {
                // 陣列為空，直接返回 null
                return null;
            }

            // 2. 隨機打散陣列 (Fisher-Yates Shuffle)
            // 從最後一個元素開始向前遍歷
            for (int i = paths.Length - 1; i > 0; i--)
            {
                // 選擇一個隨機索引 j (0 <= j <= i)
                int j = random.Next(i + 1);

                // 交換 paths[i] 和 paths[j]
                string temp = paths[i];
                paths[i] = paths[j];
                paths[j] = temp;
            }

            // 3. 遍歷打散後的陣列，檢查檔案是否存在
            foreach (string filePath in paths)
            {
                // 避免檢查空字串路徑
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    continue;
                }

                // 4. 測試檔案是否存在
                if (File.Exists(filePath))
                {
                    // 找到第一個存在的檔案，返回該路徑

                    // 可選：將 ComboBox 的選擇也設置為找到的項目
                    fileListBox.SelectedIndex = fileListBox.Items.IndexOf(filePath);

                    return filePath; // 擲回 (return) 該 string
                }
            }
            return null;             // 5. 如果遍歷完所有項目都沒有找到檔案，則返回 null
        }

        private void AlarmTitleTxt_TextChanged(object sender, EventArgs e)
        {
            theAlarmTitle = AlarmTitleTxt.Text;
            RefresAppTitle("* Settings*");

            if ((theAlarmTitle == "") || (theAlarmTitle == null))
            {
                _parentForm.notifyIcon1.Text = "AlarmTo [" + thePID + "] (Double - click to open)";
            }
            else
            {
                _parentForm.notifyIcon1.Text = "AlarmTo [" + theAlarmTitle + "] (Double - click to open)";
            }
            
        }

        private void StopAlarmSetBTN_Click(object sender, EventArgs e)
        {
            LastWindowIsVisible = true;
            AlarmOffScreen(true, false);
        }

        private int snoozeCount;
        private int snoozeMax;

        private void snoozeBTN_Click(object sender, EventArgs e)
        {
            snoozeMax = (int)soonzeNum.Value * 60;
            snoozeCount = 0;
            selectedDevice.StopAlarmSound();
            AlarmStage = 2;
            this.Hide();
        }

        private void soonzeNum_ValueChanged(object sender, EventArgs e)
        {
            if (soonzeNum.Value > 0) 
            {
                snoozeBTN.Text = "Snooze\n" + soonzeNum.Value.ToString() + " min";
                snoozeBTN.Enabled = true;
                soonzeNum.BackColor = Color.FromArgb(255, 255, 192);
            }
            else
            {
                snoozeBTN.Text = "Snooze\nDisabled";
                snoozeBTN.Enabled = false;
                soonzeNum.BackColor = Color.FromArgb(162, 162, 162);
            }
                        
        }

        private readonly string[] SupportedExtensions = { ".wav", ".mp3", ".aac", ".flac", ".m4a" };

        private void FileListComboBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // 如果有至少一個檔案符合格式，就顯示 Copy
                if (files.Any(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLower())))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FileListComboBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var file in files)
            {
                string ext = Path.GetExtension(file).ToLower();

                // 過濾格式
                if (SupportedExtensions.Contains(ext))
                {
                    // 避免加入重複項目
                    /* if (!FileListComboBox.Items.Contains(file))
                         FileListComboBox.Items.Add(file);*/
                    
                    FileListComboBox.Items.Add(file);
                }
            }

            FileListComboBox.SelectedIndex = FileListComboBox.Items.Count - 1;

        }

        private void SwW_en_BTN_Click(object sender, EventArgs e)
        {
            StartupManager.EnableStartup();
        }

        private void SwW_dis_BTN_Click(object sender, EventArgs e)
        {
            StartupManager.DisableStartup();
        }

        private void SwW_cln_BTN_Click(object sender, EventArgs e)
        {
            StartupManager.CleanupStartupEntries();
        }

        private void AlarmShowTxt_TextChanged(object sender, EventArgs e)
        {
            Graphics g = AlarmShowTxt.CreateGraphics();
            AlarmShowTxt.Font = new Font(AlarmShowTxt.Font.FontFamily, 36f, AlarmShowTxt.Font.Style);
            AdjustFontToFit(AlarmShowTxt);
        }

        private void AdjustFontToFit(System.Windows.Forms.TextBox tb)
        {
            using (Graphics g = tb.CreateGraphics())
            {
                float fontSize = tb.Font.Size;
                SizeF textSize = g.MeasureString(tb.Text, tb.Font);

                while (textSize.Width > tb.Width && fontSize > 1)
                {
                    fontSize -= 0.5f;
                    tb.Font = new Font(tb.Font.FontFamily, fontSize, tb.Font.Style);
                    textSize = g.MeasureString(tb.Text, tb.Font);
                }
            }
        }

        private void SetupComboBox(System.Windows.Forms.ComboBox theCombobox, int theMode)
        {
            theCombobox.DrawMode = DrawMode.OwnerDrawFixed;

            if (theMode == 0)
            {
                theCombobox.DrawItem += (s, e) =>
                {
                    e.DrawBackground();
                    if (e.Index >= 0)
                    {
                        string text = theCombobox.Items[e.Index].ToString();
                        using (Brush brush = new SolidBrush(e.ForeColor))
                        {
                            SizeF textSize = e.Graphics.MeasureString(text, e.Font);
                            float y = e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2;
                            float x = e.Bounds.Right - textSize.Width;
                            e.Graphics.DrawString(text, e.Font, brush, x, y);
                        }
                    }
                    e.DrawFocusRectangle();
                };
            }
            else if (theMode == 1)
            {

                theCombobox.DrawItem += (s, e) =>
                {
                    if (e.Index < 0) return;

                    e.DrawBackground();

                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;         // ⬅ 水平置中
                        sf.LineAlignment = StringAlignment.Center;     // ⬅ 垂直置中（可選）

                        e.Graphics.DrawString(
                            theCombobox.Items[e.Index].ToString(),
                            theCombobox.Font,
                            new SolidBrush(e.ForeColor),
                            e.Bounds,
                            sf);
                    }

                    e.DrawFocusRectangle();
                };
            }

                theCombobox.ItemHeight = 21;        // 下拉清單項目高度
        }

        private bool IsTesting = false;

        private void TestPlayBtn_Click(object sender, EventArgs e)
        {

            if (IsTesting)
            {
                selectedDevice.StopAlarmSound();
                TestPlayBtnReset();
            }
            else
            {
                if (!File.Exists(FileListComboBox.Text))
                {
                    MessageBox.Show("The file is missing.", "Error");
                    return;
                }

                PlayAlarm(LoopCheck.Checked, true);
                TestPlayBtn.Text = "Stop";
                IsTesting = true;
            }
        }
        private void TestPlayBtnReset()
        {
            TestPlayBtn.Text = "Test";
            IsTesting = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            theFormHelp.Show(); 
        }

        private void StopAlarmOffBTN_Click(object sender, EventArgs e)
        {
            AlarmOffScreen(LastWindowIsVisible, true);
        }

        private void iconTrack_Scroll(object sender, EventArgs e)
        {
            AlarmIconNum = iconTrack.Value;

            SelectIcon(AlarmIconNum);
            BootForm.Default.RefreshIcon(AlarmIconNum, AlarmOnOff_bool);
        }

        public void SelectIcon(int AlarmIconNum2)
        {
            switch (AlarmIconNum2)
            {
                case 0:
                    iconBox.Image = Properties.Resources.Bell_oti_off.ToBitmap();
                    break;
                case 1:
                    iconBox.Image = Properties.Resources.Bell_01_off.ToBitmap();
                    break;
                case 2:
                    iconBox.Image = Properties.Resources.Bell_02_off.ToBitmap();
                    break;
                case 3:
                    iconBox.Image = Properties.Resources.Bell_03_off.ToBitmap();
                    break;
                case 4:
                    iconBox.Image = Properties.Resources.Bell_04_off.ToBitmap();
                    break;
                case 5:
                    iconBox.Image = Properties.Resources.Bell_05_off.ToBitmap();
                    break;
            }
        }


    }
}

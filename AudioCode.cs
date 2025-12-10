using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlarmTo
{
    public class AudioDevice
    {

        private IWavePlayer waveOut;
        private AudioFileReader audioFile;
        public bool goLooping = false;
        public string FriendlyName { get; set; }
        public string ID { get; set; }

        public AudioDevice currentDevice;
        public event EventHandler<StoppedEventArgs> PlaybackStoppedEvent;

        // ComboBox 預設會呼叫 ToString() 來顯示內容
        public override string ToString()
        {
            return FriendlyName;
        }

        // --- 步驟三：在選定裝置上播放音訊 (使用 WasapiOut) ---

        /// <summary>
        /// 在指定的音訊裝置上播放音訊檔案 (使用 WasapiOut 確保精確匹配裝置 ID)。
        /// </summary>
        /// 

        public void PlayAlarmSound(string filePath, AudioDevice selectedDevice, bool LoopSound)
        {

            goLooping = LoopSound;

            try
            {
                // 2. 創建音訊檔案閱讀器，它能自動處理 WAV, MP3 等格式
                audioFile = new AudioFileReader(filePath);

                // 3. 創建 WasapiOut 播放器實例，使用裝置 ID
                using (var enumerator = new MMDeviceEnumerator())
                {
                    // 使用 Device ID 從系統中獲取 MMDevice 物件
                    MMDevice device = enumerator.GetDevice(selectedDevice.ID);

                    // 創建 WasapiOut 播放器，並傳遞我們選定的裝置
                    // 參數: 裝置, 共享模式, 循環模式 (false), 緩衝時間 (200毫秒)
                    waveOut = new WasapiOut(device, AudioClientShareMode.Shared, false, 200);
                }

                waveOut.Init(audioFile); // 4. 連接音訊流到播放器
                waveOut.PlaybackStopped += OnPlaybackStopped; // 5. 訂閱播放停止事件 (確保資源被釋放)
                waveOut.Play(); // 6. 開始播放
                
            }
            catch (Exception ex)
            {
                StopAlarmSound();
                MessageBox.Show($"An error occurred while playing audio.\n\n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 輔助函式：停止與資源清理 ---

        /// <summary>
        /// 停止播放並釋放資源。
        /// </summary>
        public void StopAlarmSound()
        {

            PlaybackStoppedEvent?.Invoke(this, null);
            goLooping = false;  // 設置旗標，告訴 OnPlaybackStopped 停止

            if (waveOut != null)
            {
                waveOut.Stop(); // 呼叫 Stop() 會觸發 OnPlaybackStopped 事件

                // 注意：waveOut 和 audioFile 的 Dispose 已經移動到 OnPlaybackStopped 的 else 分支中
                // 在外部停止模式下，我們會在那裡完成清理。
            }
            // 如果 waveOut 是 null，則 audioFile 也應該是 null，不需要額外清理。
            Application.DoEvents();

        }

        /// <summary>
        /// 處理播放停止事件，實現循環邏輯。
        /// </summary>
        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            // 如果播放器是外部呼叫 Stop() 停止的，e.Exception 將為 null
            // 且 e.PlaybackState 應該是 Stopped
            // Console.WriteLine(goLooping);
            // 只有當不是外部停止時（即檔案自然播放結束），才進行循環

            if (goLooping)
            {
                // 1. 釋放舊資源 (在重新播放前，先清理)
                if (waveOut != null)
                {
                    waveOut.Dispose();
                    waveOut = null;
                }

                if (e.Exception != null)
                {
                    MessageBox.Show($"An error occurred while playing audio.\n\n {e.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // 發生錯誤時，不循環
                    goLooping = false;
                    CleanUp();
                    return;
                }

                // 2. 將 AudioFileReader 的讀取位置設回檔案開頭
                if (audioFile != null)
                {
                    audioFile.Position = 0;

                    // 3. 重新初始化播放器並播放
                    try
                    {
                        // 必須重新獲取裝置 ID 和 MMDevice

                        using (var enumerator = new MMDeviceEnumerator())
                        {
                            MMDevice device = enumerator.GetDevice(currentDevice.ID);
                            waveOut = new WasapiOut(device, AudioClientShareMode.Shared, false, 200);
                        }

                        waveOut.Init(audioFile);
                        waveOut.PlaybackStopped += OnPlaybackStopped; // 重新訂閱事件
                        waveOut.Play();
                    }
                    catch (Exception ex) // 如果重新播放失敗
                    {
                        StopAlarmSound(); // 最終停止並清理
                        CleanUp();
                        MessageBox.Show($"An error occurred while looping audio.\n\n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                // 如果是外部停止，執行清理邏輯
                // 在這種情況下，我們可以將 StopAlarmSound() 的核心清理邏輯單獨提取，
                // 但為了簡潔，我們確保外部停止後，goLooping 標記能正確引導清理。
                // 我們將 StopAlarmSound() 的核心清理邏輯簡化。

                PlaybackStoppedEvent?.Invoke(this, null);
                CleanUp(); // 外部停止後，我們仍然需要清理

            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        /// <summary>
        /// 清除使用的資源。
        /// </summary>
        private void CleanUp()
        {
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

        }

        /// <summary>
        /// 檢查鬧鐘音訊是否正在播放。
        /// </summary>
        /// <returns>如果正在播放則為 true，否則為 false。</returns>
        public bool IsAlarmPlaying()
        {
            // 必須先檢查 waveOut 實例是否已經初始化
            if (waveOut == null)
            {
                return false;
            }

            // 查詢 PlaybackState 屬性
            return waveOut.PlaybackState == PlaybackState.Playing;
        }

    }

}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ScreenTimer
{
    /// <summary>
    /// KillProcess.xaml の相互作用ロジック
    /// </summary>
    public partial class KillProcess : Window
    {
        private DispatcherTimer _timer;
        // 秒単位
        private int min3 = 180;
        private System.Media.SoundPlayer player = null;
        string SoundFile = "sound.wav";

        private readonly Common.ProcessName processName;

        #region "最大化・最小化・閉じるボタンの非表示設定"

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int GWL_STYLE = -16;
        const int WS_SYSMENU = 0x80000;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(handle, GWL_STYLE);
            style = style & (~WS_SYSMENU);
            SetWindowLong(handle, GWL_STYLE, style);
        }

        #endregion

        public KillProcess(Common.ProcessName processName)
        {
            this.processName = processName;
            player = new System.Media.SoundPlayer(SoundFile);

            InitializeComponent();
            this.KillProcessLogic();
        }

        private void KillProcessLogic()
        {
            this.TxtProcess.Text = processName.ToString() + "のプレイしすぎです、3分後に強制終了します。";


            // 優先順位を指定してタイマのインスタンスを生成
            _timer = new DispatcherTimer(DispatcherPriority.Background);

            // インターバルを設定
            _timer.Interval = new TimeSpan(0, 0, 1);

            // タイマメソッドを設定
            _timer.Tick += (e, s) => { KillProcessTime(); };

            // 画面が閉じられるときに、タイマを停止
            this.Closing += (e, s) => { _timer.Stop(); };
            this.Closing += (e, s) =>
            {
                player.Stop();
                player.Dispose();
                player = null;
            };

            _timer.Start();
        }

        private void KillProcessTime()
        {
            this.TimerProcessKill.Content = min3 + " / 0秒になったら強制終了します。";
            min3--;
            player.Play();
            if (min3 < 0)
            {
                System.Diagnostics.Process[] ps =
                    System.Diagnostics.Process.GetProcessesByName(processName.ToString());

                foreach (System.Diagnostics.Process p in ps)
                {
                    //プロセスを強制的に終了させる
                    p.Kill();
                }
                this.Close();
            }
        }
    }
}

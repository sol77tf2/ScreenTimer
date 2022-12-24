using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ScreenTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;

        private string zenkakuSpace = "　";

        private const int hour = 1;
        private const int minute = 60;
        private const int second = 60;

        private const int minuteAdd = 60;
        private const int timerMinute = 1;

        private const string ConfigPath = "D:\\work\\ScreenTimer\\config\\";
        private const string header = "Process,PlayTime,Limit";

        private string TODAY_CSV = "";

        // 順番全てそろえること
        private MyConfig myConfigOw = new MyConfig();
        private MyConfig myConfigMs = new MyConfig();

        public MainWindow()
        {
            InitializeComponent();

            // 今日の日付のコンフィグを取得
            string today = DateTime.Now.ToString("yyyyMMdd");
            TODAY_CSV = today + ".csv";

            for (int i = 0; i < 5000; i++)
            {
                this.zenkakuSpace += "■";
                if(i % 250 == 0)
                {
                    this.zenkakuSpace += "\r\n";
                }
            }

            Start();
        }

        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            int hourMinute1 = hour * minute * second;
            int hourMinute2 = hourMinute1 * 2;

            string ow = Common.ProcessName.Overwatch.ToString() + ",0," + hourMinute1;
            string ms = Common.ProcessName.MapleStory.ToString() + ",0," + hourMinute2;
            string writeStr = header + System.Environment.NewLine +
                ow + System.Environment.NewLine +
                ms;

            DateTime dtNow = DateTime.Now;
            for (int n = 0; n < 200; n++)
            {
                using (StreamWriter writer = new StreamWriter(ConfigPath + dtNow.AddDays(n).ToString("yyyyMMdd") + ".csv", false))
                {
                    writer.WriteLine(writeStr);
                }
            }

            Start();
        }

        private void Start()
        {
            var config = new CsvConfiguration(CultureInfo.GetCultureInfo("ja-jp"))
            {
                Encoding = Encoding.UTF8
            };

            if (File.Exists(ConfigPath + TODAY_CSV))
            {
                using var csv = new CsvReader(File.OpenText(ConfigPath + TODAY_CSV), config);
                var configs = csv.GetRecords<MyConfig>().ToList();
                myConfigOw = configs[0];
                myConfigMs = configs[1];

                this.LimitOw.Content = myConfigOw.Limit + " （" + (myConfigOw.Limit / minute) + "分 ）";
                this.LimitMs.Content = myConfigMs.Limit + " （" + (myConfigMs.Limit / minute) + "分 ）";
                this.PlayTimeOw.Content = myConfigOw.PlayTime + " （" + (myConfigOw.PlayTime / minuteAdd) + "分 ）";
                this.PlayTimeMs.Content = myConfigMs.PlayTime + " （" + (myConfigMs.PlayTime / minuteAdd) + "分 ）";

                Monitoring();
            }

        }

        private void Monitoring()
        {
            // 優先順位を指定してタイマのインスタンスを生成
            _timer = new DispatcherTimer(DispatcherPriority.Background);

            // インターバルを設定
            _timer.Interval = new TimeSpan(0, timerMinute, 0);

            // タイマメソッドを設定
            _timer.Tick += (e, s) => { MonitoringProcess(); };

            // 画面が閉じられるときに、タイマを停止
            this.Closing += (e, s) => { _timer.Stop(); };

            _timer.Start();
        }

        private void MonitoringProcess()
        {
            // 実行中のすべてのプロセスを取得する
            System.Diagnostics.Process[] hProcesses = System.Diagnostics.Process.GetProcesses();

            bool writeOw = false;
            bool writeMs = false;
            // 取得できたプロセスからプロセス名を取得する
            foreach (System.Diagnostics.Process hProcess in hProcesses)
            {
                if (Common.ProcessName.Overwatch.ToString() == hProcess.ProcessName)
                {
                    writeOw = true;
                }
                if (Common.ProcessName.MapleStory.ToString() == hProcess.ProcessName)
                {
                    writeMs = true;
                }
            }
            if (writeOw)
            {
                myConfigOw.PlayTime += minuteAdd;
                this.PlayTimeOw.Content = myConfigOw.PlayTime + " （" + (myConfigOw.PlayTime / minuteAdd) + "分 ）";
                if ( myConfigOw.PlayTime / myConfigOw.Limit >= 0.85 && myConfigOw.PlayTime / myConfigOw.Limit <= 0.99)
                {
                    WarningShowMessageBox(Common.ProcessName.Overwatch);
                }
                else if(myConfigOw.PlayTime / myConfigOw.Limit >= 1)
                {
                    ProcessKillShow(Common.ProcessName.Overwatch);
                }
            }
            if (writeMs)
            {
                myConfigMs.PlayTime += minuteAdd;
                this.PlayTimeMs.Content = myConfigMs.PlayTime + " （" + (myConfigMs.PlayTime / minuteAdd) + "分 ）";
                if (myConfigMs.PlayTime / myConfigMs.Limit >= 0.85 && myConfigMs.PlayTime / myConfigMs.Limit >= 0.99)
                {
                    WarningShowMessageBox(Common.ProcessName.MapleStory);
                }
                else if (myConfigMs.PlayTime / myConfigMs.Limit >= 1)
                {
                    ProcessKillShow(Common.ProcessName.MapleStory);
                }
            }

            // ファイル書き込み
            using (StreamWriter writer = new StreamWriter(ConfigPath + TODAY_CSV, false))
            {
                writer.WriteLine(WriteStr());
            }

        }

        private string WriteStr()
        {
            // "Process,PlayTime,Limit"
            string writeStr = header + System.Environment.NewLine +
                Common.ProcessName.Overwatch.ToString() + "," + myConfigOw.PlayTime + "," + myConfigOw.Limit + System.Environment.NewLine +
                Common.ProcessName.MapleStory.ToString() + "," + myConfigMs.PlayTime + "," + myConfigMs.Limit;
            return writeStr;
        }


        private void WarningShowMessageBox(Common.ProcessName process)
        {
            new Thread(new ThreadStart(delegate
            {
                MessageBox.Show("！！警告！！" + process.ToString() + "のプレイしすぎです" + zenkakuSpace
                    , "！！警告！！" + process.ToString() + "のプレイしすぎです" + zenkakuSpace
                    , MessageBoxButton.OK
                    , MessageBoxImage.Information);
            })).Start();
        }

        private void ProcessKillShow(Common.ProcessName process)
        {
            KillProcess kp = new KillProcess(process);
            kp.ShowDialog();
            /*
            new Thread(new ThreadStart(delegate
            {
                kp.ShowDialog();
            })).Start();
            */
        }
    }
    public class MyConfig
    {
        public string Process { get; set; }
        public double PlayTime { get; set; }
        public int Limit { get; set; }
    }
}

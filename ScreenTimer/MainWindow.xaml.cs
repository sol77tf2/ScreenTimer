using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

        private const int hour = 60;
        private const int minute = 60;
        private const int second = 60;
        private const int HOUR1 = minute * second;

        private const string ConfigPath = "D:\\work\\ScreenTimer\\config\\";
        private const string Overwatch = "Overwatch";
        private const string MapleStory = "MapleStory";
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
            Start();
        }

        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            int hourMinute1 = hour * minute * second;
            int hourMinute2 = hourMinute1 * 2;

            string ow = Overwatch + ",0," + hourMinute1;
            string ms = MapleStory + ",0," + hourMinute2;
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

                this.LimitOw.Content = myConfigOw.Limit + " （" + (myConfigOw.Limit / HOUR1) + "分 ）";
                this.LimitMs.Content = myConfigMs.Limit + " （" + (myConfigMs.Limit / HOUR1) + "分 ）";
                this.PlayTimeOw.Content = myConfigOw.PlayTime;
                this.PlayTimeMs.Content = myConfigMs.PlayTime;

                Monitoring();
            }

        }

        private void Monitoring()
        {
            // 優先順位を指定してタイマのインスタンスを生成
            _timer = new DispatcherTimer(DispatcherPriority.Background);

            // インターバルを設定
            _timer.Interval = new TimeSpan(0, 3, 0);

            // タイマメソッドを設定
            _timer.Tick += (e, s) => { MonitoringProcess(); };

            // 画面が閉じられるときに、タイマを停止
            this.Closing += (e, s) => { _timer.Stop(); };

            _timer.Start();
        }

        private void MonitoringProcess()
        {
            int minute3 = 180;
            // 実行中のすべてのプロセスを取得する
            System.Diagnostics.Process[] hProcesses = System.Diagnostics.Process.GetProcesses();

            bool writeOw = false;
            bool writeMs = false;
            // 取得できたプロセスからプロセス名を取得する
            foreach (System.Diagnostics.Process hProcess in hProcesses)
            {
                if (Overwatch == hProcess.ProcessName)
                {
                    writeOw = true;
                }
                if (MapleStory == hProcess.ProcessName)
                {
                    writeMs = true;
                }
            }
            if (writeOw)
            {
                myConfigOw.PlayTime += minute3;
                this.PlayTimeOw.Content = myConfigOw.PlayTime.ToString();
                if( myConfigOw.PlayTime / myConfigOw.Limit > 0.9)
                {
                    MessageBox.Show(Overwatch, Overwatch+"のプレイしすぎです");
                }
            }
            if (writeMs)
            {
                myConfigMs.PlayTime += minute3;
                this.PlayTimeMs.Content = myConfigMs.PlayTime.ToString();
                if (myConfigMs.PlayTime / myConfigMs.Limit > 0.9)
                {
                    MessageBox.Show(MapleStory, MapleStory+"のプレイしすぎです");
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
                Overwatch + "," + myConfigOw.PlayTime + "," + myConfigOw.Limit + System.Environment.NewLine +
                MapleStory + "," + myConfigMs.PlayTime + "," + myConfigMs.Limit;
            return writeStr;
        }

    }
    public class MyConfig
    {
        public string Process { get; set; }
        public int PlayTime { get; set; }
        public int Limit { get; set; }
    }
}

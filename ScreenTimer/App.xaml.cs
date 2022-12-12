using System.Windows;
using System.Diagnostics;

namespace ScreenTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.ContextMenuStrip menu = new();
        System.Windows.Forms.NotifyIcon notifyIcon = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            // 右クリックに出すのコンテキストメニューを作成
            menu.Items.Add("アプリを終了します", null, (obj, e) => { Shutdown(); });

            // タスクトレイ上のアイコンを作成
            notifyIcon.Visible = true;
            notifyIcon.Icon = new System.Drawing.Icon(@"ico.ico"); // 今回は、icoファイルを「常にコピー」にしておいて、exeと同じ階層にicoができるようにして使用
            notifyIcon.Text = "ScreenTimer";
            notifyIcon.ContextMenuStrip = menu;

            // アイコンを押したときの処理
            notifyIcon.Click += (obj, e) =>
            {
                Debug.WriteLine("クリックされました");
            };

            // プロセス通常起動
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            menu.Dispose();
            notifyIcon.Dispose();

            base.OnExit(e);
        }
    }
}

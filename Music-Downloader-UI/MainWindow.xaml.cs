using MusicDownloader.Json;
using MusicDownloader.Library;
using MusicDownloader.Pages;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MusicDownloader
{
    public partial class MainWindow : WindowX
    {
        Music music = null;
        Setting setting;
        List<DownloadList> downloadlist = new List<DownloadList>();
        Page HomePage;
        Page DownloadPage;
        Page SettingPage;
        Page Donate = new Donate();
        System.Windows.Forms.NotifyIcon notifyicon = new System.Windows.Forms.NotifyIcon();

        #region 界面
        private void BlogButton_Click(object sender, RoutedEventArgs e) => Process.Start("https://www.nitian1207.cn/");
        private void LeftMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (frame != null)
            {
                switch (((System.Windows.Controls.HeaderedItemsControl)e.NewValue).Header)
                {
                    case "主页":
                        frame.Content = HomePage;
                        break;
                    case "下载":
                        frame.Content = DownloadPage;
                        break;
                    case "设置":
                        frame.Content = SettingPage;
                        break;
                    case "赞助":
                        frame.Content = Donate;
                        break;
                    case "反馈":
                        Process.Start("https://docs.qq.com/form/edit/DT0RraHhRZXRmYlVY");
                        break;
                    case "开源":
                        Process.Start("https://github.com/NiTian1207/Music-Downloader-UI");
                        break;
                }
            }
        }
        #endregion

        #region 事件
        private void NotifyUpdate()
        {
            var result = MessageBoxX.Show("检测到新版,是否更新", "提示:", Application.Current.MainWindow, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
            {
                MessageBoxIcon = MessageBoxIcon.Warning
            });
            if (result == MessageBoxResult.Yes)
            {
                Process.Start("https://www.nitian1207.cn/archives/496");
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void NotifyError()
        {
            var result = MessageBoxX.Show("连接更新服务器错误", "提示:", Application.Current.MainWindow, MessageBoxButton.OK, new MessageBoxXConfigurations()
            {
                MessageBoxIcon = MessageBoxIcon.Error
            });
            //Environment.Exit(0);
        }
        #endregion

        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            setting = new Setting()
            {
                SavePath = Tool.Config.Read("SavePath") ?? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                DownloadQuality = Tool.Config.Read("DownloadQuality") ?? "999000",
                IfDownloadLrc = Boolean.Parse(Tool.Config.Read("IfDownloadLrc") ?? "true"),
                IfDownloadPic = Boolean.Parse(Tool.Config.Read("IfDownloadPic") ?? "true"),
                SaveNameStyle = int.Parse(Tool.Config.Read("SaveNameStyle") ?? "0"),
                SavePathStyle = int.Parse(Tool.Config.Read("SavePathStyle") ?? "0"),
                SearchQuantity = Tool.Config.Read("SearchQuantity") ?? "100",
                TranslateLrc = int.Parse(Tool.Config.Read("TranslateLrc") ?? "0"),
                Api1 = Tool.Config.Read("Source1") ?? "",
                Api2 = Tool.Config.Read("Source2") ?? "",
                Cookie1 = Tool.Config.Read("Cookie1") ?? ""
            };
            music = new Music(setting, NotifyError, NotifyUpdate);
            HomePage = new SearchPage(music, setting);
            DownloadPage = new DownloadPage(music);
            SettingPage = new SettingPage(setting, music);
            InitializeComponent();
            frame.Content = HomePage;
            string ver = "";
            foreach (int s in music.version)
            {
                ver += s.ToString() + ".";
            }
            VerTextblock.Text = ver.Substring(0, ver.Length - 1);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (File.Exists("Error.log"))
            {
                StreamReader sr = new StreamReader("Error.log");
                StreamWriter sw = new StreamWriter("Error.log");
                //((Exception)e.ExceptionObject).
                sw.WriteLine(sr.ReadToEnd() + "\r\n" + e.ExceptionObject.ToString());
                sw.Flush();
                sw.Close();
            }
            else
            {
                StreamWriter sw = new StreamWriter("Error.log");
                sw.WriteLine(e.ExceptionObject.ToString());
                sw.Flush();
                sw.Close();
            }
            MessageBox.Show("遇到未知错误，具体信息查看 " + Environment.CurrentDirectory + "\\Error.log");
        }

        private void WindowX_ContentRendered(object sender, EventArgs e)
        {
            notifyicon.Visible = true;
            notifyicon.BalloonTipText = "Music Downloader UI";
            notifyicon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyicon.MouseClick += Notifyicon_MouseClick;
            System.Windows.Forms.MenuItem menu1 = new System.Windows.Forms.MenuItem("关闭");
            menu1.Click += Menu1_Click;
            notifyicon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] { menu1 });
            music.Update();
        }

        private void Notifyicon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (this.Visibility == Visibility.Hidden)
                {
                    this.Visibility = Visibility.Visible;
                }
                else
                {
                    this.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Menu1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void WindowX_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBoxX.Show("是否关闭程序\r\n是的,关闭\t不,最小化到托盘", "提示", this, MessageBoxButton.YesNo, new MessageBoxXConfigurations()
            {
                MessageBoxIcon = MessageBoxIcon.Info
            });
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
            }

        }
    }
}

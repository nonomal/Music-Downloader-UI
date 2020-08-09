using AduSkin.Controls.Metro;
using Microsoft.Win32;
using MusicDownloader.Json;
using MusicDownloader.Library;
using MusicDownloader.Pages;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace MusicDownloader
{
    public partial class MainWindow : Window
    {
        Music music = null;
        Setting setting;
        List<DownloadList> downloadlist = new List<DownloadList>();
        Page HomePage;
        Page DownloadPage;
        Page SettingPage;
        Page Donate = new Donate();
        System.Windows.Forms.NotifyIcon notifyicon = new System.Windows.Forms.NotifyIcon();
        //BG.ImageSource = new BitmapImage(new Uri(@"C:\Users\10240\Desktop\Background3.jpg"));

        #region 界面
        private void BlogButton_Click(object sender, RoutedEventArgs e) => Process.Start("https://www.nitian1207.cn/");
        //private void LeftMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    if (frame != null)
        //    {
        //        switch (((System.Windows.Controls.HeaderedItemsControl)e.NewValue).Header)
        //        {
        //            case "主页":
        //                frame.Content = HomePage;
        //                break;
        //            case "下载":
        //                frame.Content = DownloadPage;
        //                break;
        //            case "设置":
        //                frame.Content = SettingPage;
        //                break;
        //            case "赞助":
        //                frame.Content = Donate;
        //                break;
        //            case "反馈":
        //                Process.Start("https://docs.qq.com/form/edit/DT0RraHhRZXRmYlVY");
        //                break;
        //            case "开源":
        //                Process.Start("https://github.com/NiTian1207/Music-Downloader-UI");
        //                break;
        //            case "帮助":
        //                Process.Start("https://www.nitian1207.cn/archives/663");
        //                break;
        //        }
        //    }
        //}
        #endregion

        #region 事件
        private void NotifyUpdate()
        {
            var result = AduMessageBox.Show("检测到新版,是否更新", "提示", MessageBoxButton.YesNo);
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
            VerTextblock.Text += "(Error)";
            //var result = AduMessageBox.Show("连接更新服务器错误", "提示", Application.Current.MainWindow, MessageBoxButton.OK, new AduMessageBoxConfigurations()
            //{
            //    MessageBoxIcon = MessageBoxIcon.Error
            //});
            //Environment.Exit(0);
        }
        #endregion

        public MainWindow()
        {
            MusicDownloader.Pages.SettingPage.ChangeBlurEvent += BlurChange;
            MusicDownloader.Pages.SettingPage.SaveBlurEvent += BlurSave;
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
            music = new Music(setting);
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
            if (Tool.Config.Read("H") != null)
            {
                this.Height = Int32.Parse(Tool.Config.Read("H"));
                this.Width = Int32.Parse(Tool.Config.Read("W"));
            }
            if (!String.IsNullOrEmpty(Tool.Config.Read("Background")) && File.Exists(Tool.Config.Read("Background")))
            {
                BG.Source = new BitmapImage(new Uri(Tool.Config.Read("Background")));
            }
            if (!String.IsNullOrEmpty(Tool.Config.Read("Blur")))
            {
                Blur.Radius = double.Parse(Tool.Config.Read("Blur"));
            }
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

        async private void WindowX_ContentRendered(object sender, EventArgs e)
        {
            notifyicon.Visible = true;
            notifyicon.BalloonTipText = "Music Downloader UI";
            notifyicon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyicon.MouseClick += Notifyicon_MouseClick;
            System.Windows.Forms.MenuItem menu1 = new System.Windows.Forms.MenuItem("关闭");
            menu1.Click += Menu1_Click;
            notifyicon.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] { menu1 });
            string result = "";
            await Task.Run(() =>
            {
                result = music.Update();
            });
            if (result == "Error")
            {
                NotifyError();
            }
            if (result == "Needupdate")
            {
                NotifyUpdate();
            }
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
            Tool.Config.Write("H", ((int)this.Height).ToString());
            Tool.Config.Write("W", ((int)this.Width).ToString());
            notifyicon.Dispose();
            Application.Current.Shutdown();
        }

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(Tool.Config.Read("Close")))
            {
                switch (int.Parse(Tool.Config.Read("Close")))
                {
                    case 0:
                        MessageBoxResult result = AduMessageBox.ShowYesNo("                    确定关闭程序?", "提示", "退出", "最小化");
                        if (result == MessageBoxResult.No)
                        {
                            e.Cancel = true;
                            this.Visibility = Visibility.Hidden;
                        }
                        if (result == MessageBoxResult.Yes)
                        {
                            notifyicon.Dispose();
                            Application.Current.Shutdown();
                        }
                        break;
                    case 1:
                        e.Cancel = true;
                        this.Visibility = Visibility.Hidden;
                        break;
                    case 2:
                        notifyicon.Dispose();
                        Application.Current.Shutdown();
                        break;

                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            DropShadowEffect de = null;
            if (this.WindowState == WindowState.Normal)
            {
                de = new DropShadowEffect();
                this.BorderThickness = new Thickness(20);
                de.BlurRadius = 20;
                de.Opacity = 0.15;
                de.ShadowDepth = 0;
                this.Effect = de;
            }
            else
            {
                this.BorderThickness = new Thickness(5);
                this.Effect = null;
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Home_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)Home.IsChecked)
                {
                    frame.Content = HomePage;
                }
            }
            catch { }
        }

        private void Download_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)Download.IsChecked)
            {
                frame.Content = DownloadPage;
            }
        }

        private void Setting_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)Setting.IsChecked)
            {
                frame.Content = SettingPage;
            }
        }

        private void Donat_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)Donat.IsChecked)
            {
                frame.Content = Donate;
            }
        }

        private void Feedback_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)Feedback.IsChecked)
            {
                Process.Start("https://docs.qq.com/form/edit/DT0RraHhRZXRmYlVY");
                Home.IsChecked = true;
            }
        }

        private void Code_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)Code.IsChecked)
            {
                Process.Start("https://github.com/NiTian1207/Music-Downloader-UI");
                Home.IsChecked = true;
            }
        }

        private void Help_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)Help.IsChecked)
            {
                Process.Start("https://www.nitian1207.cn/archives/663");
                Home.IsChecked = true;
            }
        }

        private void Skin_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "图片文件(*.jpg,*.bmp,*.png)|*.jpg;*.bmp;*.png";
            ofd.ShowDialog();
            string path = ofd.FileName;
            if (path != "")
            {
                BG.Source = new BitmapImage(new Uri(path));
                Tool.Config.Write("Background", path);
            }
        }

        private void NT_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.nitian1207.cn");
        }

        private void BG_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void BlurChange(double value)
        {
            Blur.Radius = value;
        }

        public void BlurSave(double value)
        {
            Tool.Config.Write("Blur", value.ToString());
        }
    }
}

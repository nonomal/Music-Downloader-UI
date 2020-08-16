using MusicDownloader.Json;
using MusicDownloader.Library;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AduSkin.Controls.Metro;

namespace MusicDownloader.Pages
{
    public partial class SearchPage : Page
    {
        List<MusicInfo> musicinfo = null;
        MediaPlayer player = new MediaPlayer();
        Music music;
        Setting setting;
        bool isPlaying = false;
        System.Timers.Timer timer = new System.Timers.Timer(1000);

        #region 列表绑定模板
        public List<SearchListItemModel> SearchListItem = new List<SearchListItemModel>();
        public class SearchListItemModel : INotifyPropertyChanged
        {
            [DisplayName(" ")]
            public bool IsSelected { get; set; }
            [DisplayName("标题")]
            public string Title { get; set; }
            [DisplayName("歌手")]
            public string Singer { get; set; }
            [DisplayName("专辑")]
            public string Album { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged(string propertyName)
            {
                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public SearchPage(Music m, Setting s)
        {
            music = m;
            setting = s;
            InitializeComponent();
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_Pause_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isPlaying)
            {
                player.Pause();
                isPlaying = false;
                CtrlButton.Text = "\xe607";
            }
            else
            {
                try
                {
                    player.Play();
                    isPlaying = true;
                    CtrlButton.Text = "\xe61d";
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 开始播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_Play_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            string url = music.GetMusicUrl(musicinfo[List.SelectedIndex].Api, musicinfo[List.SelectedIndex].Id);
            if (string.IsNullOrEmpty(url))
            {
                AduMessageBox.Show("播放失败", "提示", MessageBoxButton.OK);
                return;
            }
            player.Open(new Uri(url));
            player.Play();
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            timer.AutoReset = true;
            isPlaying = true;
            CtrlButton.Text = "\xe61d";
        }

        /// <summary>
        /// 控制进度条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Slider.Dispatcher.Invoke(new Action(() =>
            {
                Slider.Maximum = (int)player.NaturalDuration.TimeSpan.TotalSeconds;
                Slider.Value = (int)player.Position.TotalSeconds;
            }));

        }

        /// <summary>
        /// 下载歌词
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_DownloadSelectLrc_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Download(true);
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_DownloadSelectPic_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Download(false, true);
        }

        /// <summary>
        /// 搜索按钮回车
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                searchButton_Click(this, new RoutedEventArgs());
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_SelectAll_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (SearchListItemModel m in SearchListItem)
            {
                m.IsSelected = true;
                m.OnPropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// 反选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_FanSelect_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (SearchListItemModel m in SearchListItem)
            {
                m.IsSelected = !m.IsSelected;
                m.OnPropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// 下载选中音乐按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_DownloadSelect_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Download();
        }

        /// <summary>
        /// 搜索按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Text?.Replace(" ", "") != "" && searchTextBox.Text != "搜索(歌名/歌手/ID)")
            {
                Search(searchTextBox.Text);
            }
        }

        /// <summary>
        /// 歌单按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void musiclistButton_Click(object sender, RoutedEventArgs e)
        {
            if (musiclistTextBox.Text?.Replace(" ", "") != "" && musiclistTextBox.Text != "歌单(ID/链接)")
            {
                string id = musiclistTextBox.Text;
                if (apiComboBox.SelectedIndex == 0)
                {
                    if (musiclistTextBox.Text.IndexOf("http") != -1)
                    {
                        Match match = Regex.Match(id, @"(?<=playlist\?id=)\d*");
                        id = match.Value;
                    }
                }
                if (apiComboBox.SelectedIndex == 1)
                {
                    if (id.IndexOf("https://c.y.qq.com/") != -1)
                    {
                        string qqid = Tool.GetRealUrl(id);
                        Match match = Regex.Match(qqid, @"(?<=&id=)\d*");
                        id = match.Value;
                    }
                    if (id.IndexOf("https://y.qq.com/") != -1)
                    {
                        Match match = Regex.Match(id, @"(?<=playlist/)\d*");
                        id = match.Value;
                    }
                }
                GetNeteaseMusicList(id);
            }
        }

        /// <summary>
        /// 歌单按钮回车
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void musiclistTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //    musiclistButton_Click(this, new RoutedEventArgs());
            //if (!((74 <= (int)e.Key && (int)e.Key <= 83) || (34 <= (int)e.Key && (int)e.Key <= 43) || e.Key == Key.Back))
            //{
            //    e.Handled = true;
            //}
        }

        /// <summary>
        /// 专辑按钮回车
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void albumTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //    albumButton_Click(this, new RoutedEventArgs());
            //if (!((74 <= (int)e.Key && (int)e.Key <= 83) || (34 <= (int)e.Key && (int)e.Key <= 43) || e.Key == Key.Back))
            //{
            //    e.Handled = true;
            //}
        }

        /// <summary>
        /// 专辑按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void albumButton_Click(object sender, RoutedEventArgs e)
        {
            if (albumTextBox.Text?.Replace(" ", "") != "" && albumTextBox.Text != "专辑(ID/链接)")
            {
                string id = albumTextBox.Text;
                if (apiComboBox.SelectedIndex == 0)
                {
                    if (albumTextBox.Text.IndexOf("http") != -1)
                    {
                        Match match = Regex.Match(id, @"(?<=album\?id=)\d*");
                        id = match.Value;
                    }
                }
                if (apiComboBox.SelectedIndex == 1)
                {
                    Tool.GetRealUrl(id);
                    if (id.IndexOf("https://c.y.qq.com/") != -1)
                    {
                        AduMessageBox.Show("请将链接复制到浏览器打开后再复制回程序", "提示");
                        return;
                    }
                    if (id.IndexOf("https://y.qq.com/") != -1)
                    {
                        Match match = Regex.Match(id, @"(?<=album/).*(?=\.)");
                        id = match.Value;
                    }
                }
                GetAblum(id);
            }
        }

        /// <summary>
        /// 热歌榜
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (apiComboBox.SelectedIndex == 0)
            { GetNeteaseMusicList("3778678"); }
            if (apiComboBox.SelectedIndex == 1)
            { GetQQTopList("26"); }
        }

        /// <summary>
        /// 新歌榜
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (apiComboBox.SelectedIndex == 0)
            { GetNeteaseMusicList("3779629"); }
            if (apiComboBox.SelectedIndex == 1)
            { GetQQTopList("27"); }
        }

        /// <summary>
        /// 飙升榜
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            if (apiComboBox.SelectedIndex == 0)
            { GetNeteaseMusicList("19723756"); }
            if (apiComboBox.SelectedIndex == 1)
            { GetQQTopList("62"); }
        }

        /// <summary>
        /// 原创榜
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            if (apiComboBox.SelectedIndex == 0)
            { GetNeteaseMusicList("2884035"); }
            if (apiComboBox.SelectedIndex == 1)
            {
                AduMessageBox.Show("该音源无原创榜", "提示", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="key"></param>
        private async void Search(string key)
        {
            var pb = PendingBox.Show("搜索中...", null, false, Application.Current.MainWindow, new PendingBoxConfigurations()
            {
                MinHeight = 110,
                MaxHeight = 110,
                MinWidth = 280,
                MaxWidth = 280
            });
            try
            {
                SearchListItem.Clear();
                musicinfo?.Clear();
                int api = apiComboBox.SelectedIndex + 1;
                await Task.Run(() =>
                {
                    musicinfo = music.Search(key, api);
                });
                //musicinfo = music.Search(key, apiComboBox.SelectedIndex + 1);
                if (musicinfo == null)
                {
                    pb.Close();
                    AduMessageBox.Show("搜索错误", "提示", MessageBoxButton.OK);
                    return;
                }
                foreach (MusicInfo m in musicinfo)
                {
                    SearchListItemModel mod = new SearchListItemModel()
                    {
                        Album = m.Album,
                        Singer = m.Singer,
                        IsSelected = false,
                        Title = m.Title
                    };
                    SearchListItem.Add(mod);
                }
                List.ItemsSource = SearchListItem;
                List.Items.Refresh();
                List.ScrollIntoView(List?.Items[0]);
                pb.Close();
            }
            catch
            {
                pb.Close();
                AduMessageBox.Show("搜索错误", "提示", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// 解析网易云歌单
        /// </summary>
        /// <param name="id"></param>
        private async void GetNeteaseMusicList(string id)
        {
            var pb = PendingBox.Show("解析中...", null, false, Application.Current.MainWindow, new PendingBoxConfigurations()
            {
                MinHeight = 110,
                MaxHeight = 110,
                MinWidth = 280,
                MaxWidth = 280
            });
            try
            {
                SearchListItem.Clear();
                musicinfo?.Clear();
                int api = apiComboBox.SelectedIndex + 1;
                await Task.Run(() =>
                {
                    musicinfo = music.GetMusicList(id, api);
                });
                if (musicinfo == null)
                {
                    pb.Close();
                    AduMessageBox.Show("解析错误", "提示", MessageBoxButton.OK);
                    return;
                }
                foreach (MusicInfo m in musicinfo)
                {
                    SearchListItemModel mod = new SearchListItemModel()
                    {
                        Album = m.Album,
                        Singer = m.Singer,
                        IsSelected = false,
                        Title = m.Title
                    };
                    SearchListItem.Add(mod);
                }
                List.ItemsSource = SearchListItem;
                List.Items.Refresh();
                List.ScrollIntoView(List?.Items[0]);
                pb.Close();
            }
            catch
            {
                pb.Close();
                AduMessageBox.Show("解析错误", "提示");
            }
        }

        /// <summary>
        /// 获取QQ音乐榜单
        /// </summary>
        /// <param name="id"></param>
        private async void GetQQTopList(string id)
        {
            var pb = PendingBox.Show("解析中...", null, false, Application.Current.MainWindow, new PendingBoxConfigurations()
            {
                MinHeight = 110,
                MaxHeight = 110,
                MinWidth = 280,
                MaxWidth = 280
            });
            try
            {
                SearchListItem.Clear();
                musicinfo?.Clear();
                int api = apiComboBox.SelectedIndex + 1;
                await Task.Run(() =>
                {
                    musicinfo = music.GetQQTopList(id);
                });
                if (musicinfo == null)
                {
                    pb.Close();
                    AduMessageBox.Show("解析错误", "提示", MessageBoxButton.OK);
                    return;
                }
                foreach (MusicInfo m in musicinfo)
                {
                    SearchListItemModel mod = new SearchListItemModel()
                    {
                        Album = m.Album,
                        Singer = m.Singer,
                        IsSelected = false,
                        Title = m.Title
                    };
                    SearchListItem.Add(mod);
                }
                List.ItemsSource = SearchListItem;
                List.Items.Refresh();
                pb.Close();
            }
            catch
            {
                pb.Close();
                AduMessageBox.Show("解析错误", "提示");
            }
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="ifonlydownloadlrc"></param>
        /// <param name="ifonlydownloadpic"></param>
        private async void Download(bool ifonlydownloadlrc = false, bool ifonlydownloadpic = false)
        {
            List<DownloadList> dl = new List<DownloadList>();
            for (int i = 0; i < SearchListItem.Count; i++)
            {
                if (SearchListItem[i].IsSelected)
                {
                    if (ifonlydownloadlrc)
                    {
                        dl.Add(new DownloadList
                        {
                            Id = musicinfo[i].Id.ToString(),
                            IfDownloadLrc = true,
                            IfDownloadMusic = false,
                            IfDownloadPic = false,
                            Album = musicinfo[i].Album,
                            LrcUrl = musicinfo[i].LrcUrl,
                            PicUrl = musicinfo[i].PicUrl,
                            Quality = setting.DownloadQuality,
                            Singer = musicinfo[i].Singer,
                            Title = musicinfo[i].Title,
                            Api = musicinfo[i].Api,
                            strMediaMid = musicinfo[i].strMediaMid
                        });
                    }
                    else if (ifonlydownloadpic)
                    {
                        dl.Add(new DownloadList
                        {
                            Id = musicinfo[i].Id,
                            IfDownloadLrc = false,
                            IfDownloadMusic = false,
                            IfDownloadPic = true,
                            Album = musicinfo[i].Album,
                            LrcUrl = musicinfo[i].LrcUrl,
                            PicUrl = musicinfo[i].PicUrl,
                            Quality = setting.DownloadQuality,
                            Singer = musicinfo[i].Singer,
                            Title = musicinfo[i].Title,
                            Api = musicinfo[i].Api,
                            strMediaMid = musicinfo[i].strMediaMid
                        });
                    }
                    else
                    {
                        dl.Add(new DownloadList
                        {
                            Id = musicinfo[i].Id,
                            IfDownloadLrc = setting.IfDownloadLrc,
                            IfDownloadMusic = true,
                            IfDownloadPic = setting.IfDownloadPic,
                            Album = musicinfo[i].Album,
                            LrcUrl = musicinfo[i].LrcUrl,
                            PicUrl = musicinfo[i].PicUrl,
                            Quality = setting.DownloadQuality,
                            Singer = musicinfo[i].Singer,
                            Title = musicinfo[i].Title,
                            Api = musicinfo[i].Api,
                            strMediaMid = musicinfo[i].strMediaMid
                        });
                    }
                }
            }
            if (dl.Count != 0)
            {
                int api = apiComboBox.SelectedIndex + 1;
                var pb = PendingBox.Show("请求处理中...", null, false, Application.Current.MainWindow, new PendingBoxConfigurations()
                {
                    MinHeight = 110,
                    MaxHeight = 110,
                    MinWidth = 280,
                    MaxWidth = 280
                });
                string res = "";
                await Task.Run(() =>
                {
                    res = music.Download(dl, api);
                });
                pb.Close();
                if (res != "")
                {
                    AduMessageBox.Show(res, "提示", MessageBoxButton.OK);
                }
            }
        }

        /// <summary>
        /// 解析专辑
        /// </summary>
        /// <param name="id"></param>
        private async void GetAblum(string id)
        {
            var pb = PendingBox.Show("解析中...", null, false, Application.Current.MainWindow, new PendingBoxConfigurations()
            {
                MinHeight = 110,
                MaxHeight = 110,
                MinWidth = 280,
                MaxWidth = 280
            });
            try
            {
                SearchListItem.Clear();
                musicinfo?.Clear();
                int api = apiComboBox.SelectedIndex + 1;
                await Task.Run(() =>
                {
                    musicinfo = music.GetAlbum(id, api);
                });
                if (musicinfo == null)
                {
                    pb.Close();
                    AduMessageBox.Show("解析错误", "提示");
                    return;
                }
                foreach (MusicInfo m in musicinfo)
                {
                    SearchListItemModel mod = new SearchListItemModel()
                    {
                        Album = m.Album,
                        Singer = m.Singer,
                        IsSelected = false,
                        Title = m.Title
                    };
                    SearchListItem.Add(mod);
                }
                List.ItemsSource = SearchListItem;
                List.Items.Refresh();
                List.ScrollIntoView(List?.Items[0]);
                pb.Close();
            }
            catch
            {
                pb.Close();
                AduMessageBox.Show("解析错误", "提示");
            }
        }

        /// <summary>
        /// 切换音源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void apiComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (apiComboBox.SelectedIndex == 0)
            //{
            //    apiComboBox.Foreground = new SolidColorBrush(Colors.Red);
            //}
            //if (apiComboBox.SelectedIndex == 1)
            //{
            //    apiComboBox.Foreground = new SolidColorBrush(Colors.Green);
            //}
        }

        /// <summary>
        /// 列表快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void List_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Space")
            {
                SearchListItem[List.SelectedIndex].IsSelected = !SearchListItem[List.SelectedIndex].IsSelected;
                SearchListItem[List.SelectedIndex].OnPropertyChanged("IsSelected");
            }
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key.ToString() == "X")
            {
                Download();
            }
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key.ToString() == "A")
            {
                foreach (SearchListItemModel m in SearchListItem)
                {
                    m.IsSelected = true;
                    m.OnPropertyChanged("IsSelected");
                }
            }
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key.ToString() == "R")
            {
                foreach (SearchListItemModel m in SearchListItem)
                {
                    m.IsSelected = !m.IsSelected;
                    m.OnPropertyChanged("IsSelected");
                }
            }
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key.ToString() == "P")
            {
                menu_Play_PreviewMouseDown(null, null);
            }
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key.ToString() == "P")
            {
                menu_Pause_PreviewMouseDown(null, null);
            }
        }

        /// <summary>
        /// 进度条控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
        }

        /// <summary>
        /// 进度条控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            player.Position = TimeSpan.FromSeconds(Slider.Value);
            timer.Start();
        }

        /// <summary>
        /// apiComboBox快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void apiComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key.ToString() == "Q")
            {
                if (++apiComboBox.SelectedIndex == apiComboBox.Items.Count)
                {
                    apiComboBox.SelectedIndex = 0;
                }
                else
                {
                    apiComboBox.SelectedIndex += 1;
                }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (player.Source != null)
            {
                if (CtrlButton.Text == "\xe607")
                {
                    CtrlButton.Text = "\xe61d";
                    menu_Pause_PreviewMouseDown(null, null);
                    return;
                }
                if (CtrlButton.Text == "\xe61d")
                {
                    CtrlButton.Text = "\xe607"; ;
                    menu_Pause_PreviewMouseDown(null, null);
                    return;
                }
            }
        }

        private void searchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Text == "搜索(歌名/歌手/ID)")
            {
                searchTextBox.Text = "";
                searchTextBox.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void searchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Text == "")
            {
                searchTextBox.Text = "搜索(歌名/歌手/ID)";
                searchTextBox.Foreground = new SolidColorBrush(Colors.LightGray);
            }
        }

        private void musiclistTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (musiclistTextBox.Text == "")
            {
                musiclistTextBox.Text = "歌单(ID/链接)";
                musiclistTextBox.Foreground = new SolidColorBrush(Colors.LightGray);
            }
        }

        private void musiclistTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (musiclistTextBox.Text == "歌单(ID/链接)")
            {
                musiclistTextBox.Text = "";
                musiclistTextBox.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void albumTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (albumTextBox.Text == "专辑(ID/链接)")
            {
                albumTextBox.Text = "";
                albumTextBox.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void albumTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (albumTextBox.Text == "")
            {
                albumTextBox.Text = "专辑(ID/链接)";
                albumTextBox.Foreground = new SolidColorBrush(Colors.LightGray);
            }
        }

        private void menu_MVUrl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (musicinfo[List.SelectedIndex].MVID != "0" && !string.IsNullOrEmpty(musicinfo[List.SelectedIndex].MVID))
            {
                Clipboard.SetText(music.GetMvUrl(musicinfo[List.SelectedIndex].Api, musicinfo[List.SelectedIndex].MVID));
                AduMessageBox.Show("已复制", "提示");
            }
            else
            {
                AduMessageBox.Show("无法获取Mv链接", "提示");
            }
        }
    }
}
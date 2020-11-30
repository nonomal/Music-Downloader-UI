using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace MusicDownloader.Library
{
    public static class Tool
    {
        public class Config
        {
            public static void Write(string key, string value)
            {
                bool Exist = false;
                Configuration conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                foreach (string s in conf.AppSettings.Settings.AllKeys)
                {
                    if (s == key)
                    {
                        conf.AppSettings.Settings[s].Value = value;
                        Exist = true;
                    }
                }
                if (!Exist)
                {
                    conf.AppSettings.Settings.Add(key, value);
                }
                conf.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }

            public static string Read(string key)
            {
                ConfigurationManager.RefreshSection("appSettings");
                Configuration conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                return conf.AppSettings.Settings[key]?.Value;
            }

            public static void Remove(string key)
            {
                Configuration conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                conf.AppSettings.Settings.Remove(key);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        public static List<string> GetMidText(string text, string left, string right, bool ifIncludeLR = true)//ifIncludeLR是是否包括用来定位的前后字符串
        {
            int leftindex = 0;
            int rightindex = -right.Length;
            List<string> re = new List<string>();
            while (text.IndexOf(left, rightindex + right.Length) != -1)
            {
                leftindex = text.IndexOf(left, rightindex + right.Length);
                rightindex = text.IndexOf(right, leftindex + left.Length);
                if (ifIncludeLR)
                {
                    re.Add(left + text.Substring(leftindex + left.Length, rightindex - leftindex - left.Length) + right);
                }
                else
                {
                    re.Add(text.Substring(leftindex + left.Length, rightindex - leftindex - left.Length));
                }
            }
            return re;
        }

        public static string GetRealUrl(string url)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 20000;
                req.AllowAutoRedirect = true;
                string _url = req.GetResponse().ResponseUri.AbsoluteUri;
                req.Abort();
                return _url;
            }
            catch
            {
                return null;
            }
        }

        public class WebClientPro : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
                request.Timeout = 1000 * 8;//单位为毫秒
                return request;
            }
        }
        public static void PngToJpg(string source)
        {
            Bitmap im = new Bitmap(source);
            EncoderParameters eps = new EncoderParameters(1);
            EncoderParameter ep = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);
            eps.Param[0] = ep;
            ImageCodecInfo jpsEncodeer = GetEncoder(ImageFormat.Jpeg);
            im.Save(source.Replace(Path.GetFileNameWithoutExtension(source), Path.GetFileNameWithoutExtension(source) + "-T"), jpsEncodeer, eps);
            im.Dispose();
            ep.Dispose();
            eps.Dispose();
            File.Delete(source);
            File.Move(source.Replace(Path.GetFileNameWithoutExtension(source), Path.GetFileNameWithoutExtension(source) + "-T"), source);
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

    }
}
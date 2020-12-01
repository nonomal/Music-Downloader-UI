using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace MusicDownloader.Library
{
    public class HttpHelper
    {
        public static string Post(string Url, Dictionary<string, string> Headers, Dictionary<string, string> KeyValue)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            string content = "";
            foreach (KeyValuePair<string, string> kv in KeyValue)
            {
                content += kv.Key + "=" + kv.Value + "&";
            }
            content = content.Substring(0, content.Length - 1);
            byte[] data = Encoding.UTF8.GetBytes(content);
            req.ContentLength = data.Length;
            foreach (KeyValuePair<string, string> hrh in Headers)
            {
                if (hrh.Key == "Referer")
                {
                    req.Referer = hrh.Value;
                    continue;
                }
                req.Headers[hrh.Key] = hrh.Value;
            }
            req.GetRequestStream().Write(data, 0, data.Length);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            StreamReader reader = new StreamReader(res.GetResponseStream());
            string retString = reader.ReadToEnd();
            return retString;
        }

        public static string Post(string Url, Dictionary<string, string> Headers, string Json)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            foreach (KeyValuePair<string, string> hrh in Headers)
            {
                req.Headers.Add(hrh.Key, hrh.Value);
            }
            req.Method = "POST";
            req.ContentType = "application/json";
            byte[] data = Encoding.UTF8.GetBytes(Json);
            req.ContentLength = data.Length;
            StreamWriter writer = new StreamWriter(req.GetRequestStream(), Encoding.UTF8);
            writer.Write(data);
            writer.Flush();
            writer.Close();
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            StreamReader reader = new StreamReader(res.GetResponseStream());
            string retString = reader.ReadToEnd();
            return retString;
        }
    }
}

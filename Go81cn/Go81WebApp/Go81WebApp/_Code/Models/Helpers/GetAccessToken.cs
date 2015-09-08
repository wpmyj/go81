using Go81WebApp.Models.数据模型.微信数据模型;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Go81WebApp._Code.Models.Helpers
{
    public class GetAccessToken
    {
        public static string AppID = ConfigurationManager.AppSettings["微信AppID"];
        public static string AppSecret = ConfigurationManager.AppSettings["微信AppSecret"];

        private static DateTime GetAccessToken_Time;
        /// <summary>
        /// 过期时间为7200秒
        /// </summary>
        private static int Expires_Period = 7200;
        /// <summary>
        /// 
        /// </summary>
        private static string mAccessToken;
        /// <summary>
        /// 
        /// </summary>
        public static string AccessToken
        {
            get
            {
                //如果为空，或者过期，需要重新获取
                if (string.IsNullOrEmpty(mAccessToken) || HasExpired())
                {
                    //获取
                    mAccessToken = GetAccessTokenSting();
                }

                return mAccessToken;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetAccessTokenSting()
        {
            var thistoken = webRequestPost("https://api.weixin.qq.com/cgi-bin/token", "grant_type=client_credential&appid=" + AppID + "&secret=" + AppSecret);
            微信Token token = (微信Token)JsonConvert.DeserializeObject(thistoken, typeof(微信Token));

            if (token != null)
            {
                var access_token = token.access_token;
                if (access_token != null)
                {
                    GetAccessToken_Time = DateTime.Now;
                    if (token.expires_in != null)
                    {
                        Expires_Period = int.Parse(token.expires_in);
                    }
                    return token.access_token;
                }
                else
                {
                    GetAccessToken_Time = DateTime.MinValue;
                }
            }

            return null;
        }
        /// <summary>
        /// 判断Access_token是否过期
        /// </summary>
        /// <returns>bool</returns>
        private static bool HasExpired()
        {
            if (GetAccessToken_Time != null)
            {
                //过期时间，允许有一定的误差，一分钟。获取时间消耗
                if (DateTime.Now > GetAccessToken_Time.AddSeconds(Expires_Period).AddSeconds(-60))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>  
        /// Post 提交调用抓取  
        /// </summary>  
        /// <param name="url">提交地址</param>  
        /// <param name="param">参数</param>  
        /// <returns>string</returns>  
        public static string webRequestPost(string url, string param)
        {
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(param);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url + "?" + param);
            req.Method = "Post";
            req.Timeout = 120 * 1000;
            req.ContentType = "application/x-www-form-urlencoded;";
            req.ContentLength = bs.Length;

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
                reqStream.Flush();
            }
            using (WebResponse wr = req.GetResponse())
            {
                //在这里对接收到的页面内容进行处理   

                Stream strm = wr.GetResponseStream();

                StreamReader sr = new StreamReader(strm, System.Text.Encoding.UTF8);

                string line;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                while ((line = sr.ReadLine()) != null)
                {
                    sb.Append(line + System.Environment.NewLine);
                }
                sr.Close();
                strm.Close();
                return sb.ToString();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace MailApiPostClass
{
    public class MailApiPost
    {
        /// <summary>
        /// 短信发送接口
        /// </summary>
        /// <param name="UserNumber">电话号码</param>
        /// <param name="MessageContent">短信内容</param>
        /// <returns></returns>
        public static string PostDataGetHtml(string UserNumber, string MessageContent)
        {
            try
            {
                string SpCode = ConfigurationManager.AppSettings["企业编号"];//企业编号
                string LoginName = ConfigurationManager.AppSettings["用户名称"];//用户名称
                string Password = ConfigurationManager.AppSettings["用户密码"];//用户密码
                string SerialNumber = DateTime.Now.ToString("yyyyMMddhhmmss") + DateTime.Now.Millisecond.ToString();//20位唯一序列号
                System.Random R = new Random();
                for (int i = 0; i < 3; i++)
                {
                    SerialNumber += R.Next(10).ToString();
                }

                string  postData = "SpCode=" + SpCode + "&LoginName=" + LoginName + "&Password=" + Password + "&MessageContent=" + MessageContent + "&UserNumber=" + UserNumber + "&SerialNumber=" + SerialNumber + "&ScheduleTime=&f=1";

                byte[] data = Encoding.GetEncoding(936).GetBytes(postData);

                string uri = "http://sc.ums86.com:8899/sms/Api/Send.do";
                Uri uRI = new Uri(uri);
                HttpWebRequest req = WebRequest.Create(uRI) as HttpWebRequest;
                req.Method = "POST";
                req.KeepAlive = true;
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = data.Length;
                req.AllowAutoRedirect = true;

                Stream outStream = req.GetRequestStream();
                outStream.Write(data, 0, data.Length);
                outStream.Close();

                HttpWebResponse res = req.GetResponse() as HttpWebResponse;
                Stream inStream = res.GetResponseStream();
                StreamReader sr = new StreamReader(inStream, Encoding.GetEncoding(936));
                // sr  = HttpUtility.UrlEncode(sr, System.Text.Encoding.GetEncoding(936))

                string htmlResult = sr.ReadToEnd();

                return htmlResult;
            }
            catch (Exception ex)
            {
                return "网络错误：" + ex.Message.ToString();
            }
        }

        //public static string SendMessageByIndustry(long[] tels)
        //{
                
        //}


    }
}
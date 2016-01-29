using System.Text;
using System.Net;
using System.IO;
using DTAPI;
using System;

namespace DTAPI
{
    /// <summary>
    /// HTTP处理OTA API的基础类
    /// </summary>
    public class HTTPHandler
    {
        private HttpWebRequest request;

        public HttpWebRequest Request
        {
            get
            {
                return request;
            }

            set
            {
                request = value;
            }
        }

        // OTA API的URL
        public const string requestURL = "http://120.24.75.164:8999/OtaInterfaceConnections/OtaDataInterFace/getOtaMethod";
        // 访问OTA API的帐户密钥
        public const string accountKey = "jingrui2004";

        public static HttpWebRequest GetJsonRequest(string method)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestURL);

            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Method = method;
            
            return request;
        }

        public static HttpWebRequest GetJsonRequest(string url, string method)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Method = method;

            return request;
        }

        public static GenericRequest GenerateRequetData(string methodName)
        {
            GenericRequest request = new GenericRequest();
            RequestHeader myHeader = new RequestHeader();

            //填充Header中ServiceName
            myHeader.ServiceName = methodName;

            //获取Sign
            string signString = SignUtil.CreateSign(myHeader, accountKey);
            //填充Header中Sign
            myHeader.Sign = signString;

            request.Head = myHeader;

            return request;
        }

        /// <summary>
        /// 发送POST请求，并返回其应答
        /// </summary>
        /// <param name="requestString">请求的JSON字符串</param>
        /// <returns>应答的JSON字符串</returns>
        public static string SendPostRequest(string requestString)
        {
            string responseData = string.Empty;

            //创建 HTTP request
            HttpWebRequest request = GetJsonRequest("POST");      
            
            //请求字节流
            byte[] postBytes = Encoding.UTF8.GetBytes(requestString);
            request.ContentLength = postBytes.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(postBytes, 0, postBytes.Length);
            dataStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // 应答 HTTP status
            string status = response.StatusDescription;
            // HTTP status 200 OK
            if (status.Equals("OK"))
            {
                // 读取应答流
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                // 读取应答的内容
                responseData = reader.ReadToEnd();

                // 清除各种流
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            else
            {
                responseData = status;
            }

            return responseData;
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="requestString">请求的字符串</param>
        /// <returns>应答的字符串</returns>
        public static string SendPostRequest(string url, string requestString)
        {
            string responseData = string.Empty;

            //创建 HTTP request
            HttpWebRequest request = GetJsonRequest(url, "POST");

            //请求字节流
            byte[] postBytes = Encoding.UTF8.GetBytes(requestString);
            request.ContentLength = postBytes.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(postBytes, 0, postBytes.Length);
            dataStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // 应答 HTTP status
            string status = response.StatusDescription;
            // HTTP status 200 OK
            if (status.Equals("OK"))
            {
                // 读取应答流
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                // 读取应答的内容
                responseData = reader.ReadToEnd();

                // 清除各种流
                reader.Close();
                dataStream.Close();
                response.Close();
            }
            else
            {
                responseData = status;
            }

            return responseData;
        }
    }
}

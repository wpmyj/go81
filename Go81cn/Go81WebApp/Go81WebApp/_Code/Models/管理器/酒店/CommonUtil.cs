using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAPI
{
    /// <summary>
    /// 公共数据类
    /// </summary>
    public class CommonUtil
    {
        public static string GetCommonRequestData(string serviceName)
        {
            string requestData;
            GenericRequest request = HTTPHandler.GenerateRequetData(serviceName);

            JObject jObj = JObject.FromObject(request);

            //GetCommonRequest Body
            jObj.Add("body", "[]");

            //Json数据
            //requestData = jObj.ToString();
            requestData = JsonUtil.GetJsonFromObject(jObj);

            return requestData;
        }

        /// <summary>
        /// 获取公共部分数据
        /// </summary>
        /// <param name="serviceName">接口名称</param>
        /// <returns></returns>
        public static string GetCommonResponseData(string serviceName)
        {
            string responseData = string.Empty;
            string requestData = GetCommonRequestData(serviceName);

            if (!requestData.Equals(string.Empty))
            {
                responseData = HTTPHandler.SendPostRequest(requestData);
            }

            return responseData;
        }
    }
}

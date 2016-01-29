using Newtonsoft.Json;
using System.IO;

namespace DTAPI
{
    /// <summary>
    /// JSON操作基础类
    /// </summary>
    public class JsonUtil
    {
        /// <summary>
        /// 将JSON格式文件的内容输出为JSON字符串
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string LoadJson(string file)
        {
            string jsonString = string.Empty;

            using (StreamReader r = new StreamReader(file))
            {
                jsonString = r.ReadToEnd();
            }

            return jsonString;
        }

        /// <summary>
        /// 将JSON字符串转为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static object GetObjectFromJson<T>(string jsonData)
        {
            var obj = JsonConvert.DeserializeObject<T>(jsonData);

            return obj;
        }

        /// <summary>
        /// 将对象转为JSON字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetJsonFromObject(object obj)
        {
            CustomJsonConverter convert = new CustomJsonConverter();
            convert.PropertyNameCase = ConverterPropertyNameCase.CamelCase;

            string jsonData = JsonConvert.SerializeObject(obj, convert);

            return jsonData;
        }
    }
}

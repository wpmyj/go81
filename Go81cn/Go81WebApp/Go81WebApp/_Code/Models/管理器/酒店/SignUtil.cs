using System;
using System.Security.Cryptography;
using System.Text;

namespace DTAPI
{
    /// <summary>
    /// 签名类
    /// </summary>
    public class SignUtil
    {
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="header">请求头部</param>
        /// <param name="accountKey">API账号密钥</param>
        /// <returns>签名</returns>
        public static string CreateSign(RequestHeader header, string accountKey)
        {
            if (accountKey == null || "".Equals(accountKey))
            {
                throw new Exception("缺少API帐户密钥");
            }
            string[] originalArray =
            {
                "Version=" + header.Version,
                "AccountID=" + header.AccountId,
                "ServiceName=" + header.ServiceName,
                "ReqTime=" + header.ReqTime
            };
            string[] sortedArray = BubbleSort(originalArray);
            string sign = GetMD5ByArray(sortedArray, accountKey, "utf-8");
            return sign;
        }

        /// <summary>
        /// 数组冒泡排序
        /// </summary>
        /// <param name="originalArray">待排序字符串数组</param>
        /// <returns>经过冒泡排序过的字符串数组</returns>
        public static string[] BubbleSort(string[] originalArray)
        {
            int i, j; // 交换标志
            string temp;
            bool exchange;

            for (i = 0; i < originalArray.Length; i++) // 最多做R.Length-1趟排序
            {
                exchange = false; // 本趟排序开始前，交换标志应为假
                for (j = originalArray.Length - 2; j >= i; j--)
                {
                    if (originalArray[j + 1].CompareTo(originalArray[j]) < 0)// 交换条件
                    {
                        temp = originalArray[j + 1];
                        originalArray[j + 1] = originalArray[j];
                        originalArray[j] = temp;
                        exchange = true; // 发生了交换，故将交换标志置为真
                    }
                }
                if (!exchange) // 本趟排序未发生交换，提前终止算法
                {
                    break;
                }
            }
            return originalArray;
        }

        public static string GetMD5ByArray(string[] sortedArray, string key, string charset)
        {
            // 构造待md5摘要字符串
            StringBuilder prestr = new StringBuilder();

            for (int i = 0; i < sortedArray.Length; i++)
            {
                if (i == sortedArray.Length - 1)
                {
                    prestr.Append(sortedArray[i]);
                }
                else
                {
                    prestr.Append(sortedArray[i] + "&");
                }
            }
            prestr.Append(key);// 此处key为上面的innerKey
            return GetMD5(prestr.ToString(), charset);
        }

        public static string GetMD5(string input, string charset)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = Encoding.GetEncoding(charset).GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            // Convert array bytes to string haxadecimal
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}

using Go81WebApp.Models.数据模型;
using System;
using System.Configuration;
using System.IO;
using System.Web;

namespace Go81WebApp.Models.管理器
{
    public static class 上传管理
    {
        public static string 上传文件根路径 = ConfigurationManager.AppSettings["上传文件根路径"];
        private const string 文件路径格式 = "{0}/{1}/{2}/{3}/{4}/{5}/";
        public static string 获取上传路径<T>(媒体类型 mt, 路径类型 pt) where T : 基本数据模型
        {
            var tn = typeof(T).Name;
            var mn = mt.ToString();
            var d = DateTime.Now;
            var p = string.Format(文件路径格式, 上传文件根路径, tn, mn, d.Year, d.Month, d.Day);
            switch (pt)
            {
                case 路径类型.不带域名根路径: return VirtualPathUtility.ToAbsolute(p);
                case 路径类型.应用程序相对路径: return p;
                case 路径类型.服务器本地路径: return HttpContext.Current.Server.MapPath(p);
            }
            return p;
        }
        public static string 获取文件名(string filename)
        {
            return filename.Insert(0,Guid.NewGuid().ToString());
        }
        public static string 获取文件名(Stream fs)
        {
            return BitConverter.ToString((new System.Security.Cryptography.MD5CryptoServiceProvider()).ComputeHash(fs)).Replace("-", "");
        }
        public static string 保存上传文件<T>(HttpPostedFileBase f, 媒体类型 mt) where T : 基本数据模型
        {
            return null;
        }
    }
    public enum 媒体类型
    {
        未知 = 0,
        图片 = 1,
        音频 = 2,
        视频 = 3,
        文档 = 4,
        标书 = 5,
        头像 = 101,
        附件 = 102,
        其他 = 9999,
    }
    public enum 路径类型
    {
        /// <summary>
        /// 带有域名和虚拟目录可直接从浏览器访问的形式，如 http://192.168.1.9:9091/files/uploads/image
        /// </summary>
        带域名完整路径 = 0,
        /// <summary>
        /// 不带有域名的虚拟目录路径，如 /files/uploads/image
        /// </summary>
        不带域名根路径 = 1,
        /// <summary>
        /// 相对应用程序根目录的形式，如 ~/files/uploads
        /// </summary>
        应用程序相对路径 = 2,
        /// <summary>
        /// 本地带驱动器号的形式，如 C:\www\root\files\uploads
        /// </summary>
        服务器本地路径 = 3,
    }
}
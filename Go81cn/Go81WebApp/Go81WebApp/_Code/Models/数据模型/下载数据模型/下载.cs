
using Helpers;

namespace Go81WebApp.Models.数据模型.内容数据模型
{
    public class 下载 : 内容基本数据
    {
        public 下载类型 下载类型 { get; set; }
        public string 下载验证码 { get; set; }
        public int 下载次数 { get; set; }
        public 下载()
        {
            this.下载次数 = RandomString.ClickStartNum();
        }
    }

    public enum 下载类型
    {
        普通 = 0,
        标书 = 1,
        工具软件 = 2,
    }
}
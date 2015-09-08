
namespace Go81WebApp.Models.数据模型.推送数据模型
{
    public class 推送模板 : 基本数据模型
    {
        public 推送类型 推送类型 { get; set; }
        public string 标题 { get; set; }
        public string 内容 { get; set; }
    }
    public enum 推送类型{
        未指定 = 0,
        短信消息 = 1,
        微信消息 = 2,
        邮件消息 = 3,
        站内消息 = 4,
    }
}
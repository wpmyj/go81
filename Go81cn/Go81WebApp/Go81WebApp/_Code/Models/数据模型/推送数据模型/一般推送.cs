using Go81WebApp.Models.管理器.推送管理;
using Go81WebApp.Models.数据模型.用户数据模型;
using System.Collections.Generic;

namespace Go81WebApp.Models.数据模型.推送数据模型
{
    public class 一般推送 : 基本数据模型
    {
        public 用户链接<运营团队> 操作人员 { get; set; }
        public _审核数据 审核数据 { get; set; }
        public 短信推送数据 短信推送数据 { get; set; }
        public 微信推送数据 微信推送数据 { get; set; }
        public 电子邮件推送数据 电子邮件推送数据 { get; set; }
        public 站内消息推送数据 站内消息推送数据 { get; set; }
        public class _审核数据
        {
            public 用户链接<运营团队> 审核人员 { get; set; }
            public 推送信息状态 推送信息状态 { get; set; }
            public _审核数据() { this.审核人员 = new 用户链接<运营团队>(); }
        }
        public 一般推送()
        {
            this.操作人员 = new 用户链接<运营团队>();
            this.审核数据 = new _审核数据();
            this.短信推送数据 = new 短信推送数据();
            this.微信推送数据 = new 微信推送数据();
            this.电子邮件推送数据 = new 电子邮件推送数据();
            this.站内消息推送数据 = new 站内消息推送数据();
        }
    }
    public enum 推送信息状态
    {
        未提交 = 0,
        未审核 = 1,
        审核通过 = 2,
        审核未通过 = 3,
    }
    public class 短信推送数据
    {
        public List<string> 收信人列表 { get; set; }
        public string 标题 { get; set; }
        public string 内容 { get; set; }
        public 短信推送数据() { this.收信人列表 = new List<string>(); }
    }
    public class 微信推送数据
    {
        public List<string> 收信人列表 { get; set; }
        public string 标题 { get; set; }
        public string 内容 { get; set; }
        public 微信推送数据() { this.收信人列表 = new List<string>(); }
    }
    public class 电子邮件推送数据
    {
        public List<string> 收信人列表 { get; set; }
        public string 标题 { get; set; }
        public string 内容 { get; set; }
        public 电子邮件推送数据() { this.收信人列表 = new List<string>(); }
    }
    public class 站内消息推送数据
    {
        public List<用户链接<用户基本数据>> 收信人列表 { get; set; }
        public string 标题 { get; set; }
        public string 内容 { get; set; }
        public 站内消息推送数据() { this.收信人列表 = new List<用户链接<用户基本数据>>(); }
    }
    public class 一般推送链接
    {
        public long 一般推送ID { get; set; }
        public 一般推送 一般推送 { get { return 一般推送管理.查找一般推送(一般推送ID); } }
        public 一般推送链接() { this.一般推送ID = -1; }
    }
}
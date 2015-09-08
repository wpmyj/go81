
namespace Go81WebApp.Models.数据模型.消息数据模型
{
    public class 站内消息 : 对话基本数据
    {
        public 重要程度 重要程度 { get; set; }
        public 消息类型 消息类型 { get; set; }
        public 对话参与者链接 收信人 { get; set; }
        public 站内消息()
        {
            this.收信人 = new 对话参与者链接();
        }
    }
    public enum 消息类型
    {
        未指定 = 0,
        普通 = 100,
        采购信息 = 200,
        推荐专家和供应商 = 300,
        公告传递 = 400,
    }
}
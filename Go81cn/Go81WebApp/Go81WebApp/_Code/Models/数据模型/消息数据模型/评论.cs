
namespace Go81WebApp.Models.数据模型.消息数据模型
{
    public class 评论 : 对话基本数据
    {
        public 数据链接 关联对象 { get; set; }
        public 评论() { this.关联对象 = new 数据链接(); }
    }
}
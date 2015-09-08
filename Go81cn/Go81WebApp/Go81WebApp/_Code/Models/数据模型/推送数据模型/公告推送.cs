using Go81WebApp.Models.管理器.推送管理;
using Go81WebApp.Models.数据模型.内容数据模型;

namespace Go81WebApp.Models.数据模型.推送数据模型
{
    public class 公告推送 : 一般推送
    {
        public 公告链接 关联公告 { get; set; }
        public 公告推送() { this.关联公告 = new 公告链接(); }
    }
    public class 公告推送链接
    {
        public long 公告推送ID { get; set; }
        public 公告推送 公告推送 { get { return 公告推送管理.查找公告推送(公告推送ID); } }
        public 公告推送链接() { this.公告推送ID = -1; }
    }
}
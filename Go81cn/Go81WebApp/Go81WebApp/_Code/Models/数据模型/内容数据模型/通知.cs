using Go81WebApp.Models.管理器;

namespace Go81WebApp.Models.数据模型.内容数据模型
{
    public class 通知 : 内容基本数据
    {
        public _内容来源 通知来源 { get; set; }
        public _通知信息 通知信息 { get; set; }
        public class _通知信息
        {
            public 通知所属 通知所属 { get; set; }
        }
        public enum 通知所属
        {
            未指定 = 0,
            网站 = 1,
            单位 = 2,
            供应商 = 3,
            专家 = 4,
            网上竞标 = 11,
            黑名单 = 21,
            光荣榜 = 22,
            其他 = 99,
        }
        public 通知()
        {
            this.通知信息 = new _通知信息();
            this.审核数据 = new _审核数据();
        }
    }
    public class 通知链接
    {
        public long 通知ID { get; set; }
        public 通知 通知 { get { return 通知管理.查找通知(通知ID); } }
        public 通知链接() { this.通知ID = -1; }
    }
}
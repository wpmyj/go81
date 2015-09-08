
namespace Go81WebApp.Models.数据模型.消息数据模型
{
    public class 建议 : 对话基本数据
    {
        public 对话参与者链接 受理单位 { get; set; }
        public 处理状态 处理状态 { get; set; }
        public 建议()
        {
            this.受理单位 = new 对话参与者链接();
        }
    }
}
using System.ComponentModel.DataAnnotations;
namespace Go81WebApp.Models.数据模型.内容数据模型
{
    public class 政策法规 : 内容基本数据
    {
        public _内容来源 法规来源 { get; set; }
        public _政策法规信息 政策法规信息 { get; set; }
        public class _政策法规信息
        {
            [Required(ErrorMessage = "发布机关必须填写")]
            public string 发布机关 { get; set; }
            public string 发文号 { get; set; }
            public string 法规类型 { get; set; }
        }
        public 政策法规()
        {
            this.政策法规信息 = new _政策法规信息();
            this.审核数据 = new _审核数据();
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Go81WebApp.Models.数据模型.内容数据模型
{
    public class 新闻 : 内容基本数据
    {
        public _内容来源 新闻来源 { get; set; }
        public bool 图片新闻 { get { return null != this.内容主体.图片 && 0 != this.内容主体.图片.Count && !string.IsNullOrWhiteSpace(this.内容主体.图片[0]); } }
        public 新闻()
        {
            this.新闻来源 = new _内容来源();
            this.审核数据 = new _审核数据();
        }
    }
    public class _内容来源
    {
        public string 来源名称 { get; set; }

        [RegularExpression(@"^[A-Za-z]+://[A-Za-z0-9-_]+\.[A-Za-z0-9-_%&?/.=]+$", ErrorMessage = "url不正确(http://www.go81.cn)")]
        public string 来源链接 { get; set; }
    }
}
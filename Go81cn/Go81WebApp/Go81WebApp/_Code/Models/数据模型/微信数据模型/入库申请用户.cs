using Go81WebApp.Models.数据模型;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Go81WebApp._Code.Models.数据模型.微信数据模型
{
    public class 入库申请用户:基本数据模型
    {
        [Required(AllowEmptyStrings=false,ErrorMessage="请填写您的姓名")]
        public string 用户名称 { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "请填写电话号码")]
        [RegularExpression(@"\d{2,5}-\d{7,8}|^(13|14|15|16|18|19)\d{9}$", ErrorMessage = "*正确格式为：区号-电话号码或移动号码")]
        public string 联系电话 { get; set; }
        public 申请类型 申请类型 { get; set; }
    }
    public enum 申请类型
    {
        供应商 = 1,
        专家 = 2,
    }
}
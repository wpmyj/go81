using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型
{
    public class 机票代售点 : 基本数据模型
    {
        public 用户链接<供应商> 所属供应商 { get; set; }
        [Required(ErrorMessage = "*请输入代售点名称")]
        public string 代售点名称 { get; set; }
        [Required(ErrorMessage = "*请输入代售点地址")]
        public string 地址 { get; set; }
        [Required(ErrorMessage = "*请输入代售点所属商圈")]
        public string 所属商圈 { get; set; }
        [Required(ErrorMessage = "*请对代售点作简单")]
        public string 简介 { get; set; }
        [Required(ErrorMessage = "*请输入代售点联系电话")]
        [RegularExpression(@"^(13|14|15|16|18|19)\d{9}$", ErrorMessage = "*请输入正确的手机格式")]
        public string 联系电话 { get; set; }
        [Required(ErrorMessage = "*请输入代售点交通信息")]
        public string 交通信息 { get; set; }
        public List<string> 照片 { get; set; }
        public _地域 所属地域 { get; set; }
        public double[] 地理位置 { get; set; }
        public bool 送票服务 { get; set; }
        public _审核数据 审核数据 { get; set; }
        public class _审核数据
        {
            public 审核状态 审核状态 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 审核时间 { get; set; }
            public 用户链接<运营团队> 审核者 { get; set; }
            public string 审核不通过原因 { get; set; }
            public _审核数据() { this.审核者 = new 用户链接<运营团队>(); }
        }
        public 机票代售点()
        {
            this.所属供应商 = new 用户链接<供应商>();
            this.所属地域 = new _地域();
            this.照片 = new List<string>();
            this.审核数据 = new _审核数据();
        }
    }
}

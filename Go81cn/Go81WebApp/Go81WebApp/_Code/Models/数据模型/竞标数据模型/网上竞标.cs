using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.数据模型.项目数据模型;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型.竞标数据模型
{
    public class 网上竞标 : 基本数据模型
    {
        public string 项目编号 { get; set; }
        public string 项目名称 { get; set; }

        [Required(ErrorMessage = "请选择所属行业")]
        public string 所属行业 { get; set; }

        public string 商品图片 { get; set; }

        [Required(ErrorMessage = "请输入商品名称")]
        public string 商品名称 { get; set; }

        [Required(ErrorMessage = "请输入商品型号")]
        public string 商品型号 { get; set; }

        public string 参考品牌 { get; set; }

        [Required(ErrorMessage = "请输入商品描述")]
        public string 商品描述 { get; set; }

        [Required(ErrorMessage = "请输入商品数量")]
        public int 商品需求数量 { get; set; }

        public string 计量单位 { get; set; }

        //[DataType(DataType.Currency, ErrorMessage = "请输入正确的价格")]
        //[Required(ErrorMessage = "请输入结束时间")]
        public decimal 起始价格 { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [Required(ErrorMessage = "请输入结束时间")]
        [DataType(DataType.DateTime, ErrorMessage = "请输入正确的结束时间")]
        public DateTime 报价结束时间 { get; set; }

        [Required(ErrorMessage = "请输入送货地址")]
        public string 送货地址 { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [Required(ErrorMessage = "请输入交货时间")]
        [DataType(DataType.DateTime, ErrorMessage = "请输入正确的交货时间")]
        public DateTime 交货时间 { get; set; }

        public _联系方式 联系方式 { get; set; }

        public string 售后服务要求 { get; set; }
        public string 维保质保要求 { get; set; }
        public string 备注 { get; set; }
        public List<_报价供应商> 报价供应商列表 { get; set; }

        public class _报价供应商
        {
            public decimal 报价 { get; set; }    //单价
            public decimal 税率 { get; set; }
            public decimal 运杂费 { get; set; }
            public string 发货地点 { get; set; }
            public decimal 其他费用 { get; set; }
            public string 售后服务 { get; set; }
            public string 备注 { get; set; }
            public decimal 总价 { get; set; }    //价税合计
            public 用户链接<供应商> 报价供应商 { get; set; }
            public _报价供应商()
            {
                报价供应商 = new 用户链接<供应商>();
            }
        }

        public decimal 最终价格 { get; set; }

        public 用户链接<供应商> 中标供应商链接 { get; set; }
        
        public 网上竞标()
        {
            报价供应商列表 = new List<_报价供应商>();
            中标供应商链接 = new 用户链接<供应商>();
        }
    }
}
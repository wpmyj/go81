using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.用户数据模型;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Helpers;

namespace Go81WebApp.Models.数据模型.商品数据模型
{
    public class 商品 : 基本数据模型
    {
        public _审核数据 审核数据 { get; set; }
        public _商品信息 商品信息 { get; set; }
        public _商品数据 商品数据 { get; set; }
        public _销售信息 销售信息 { get; set; }
        public _采购信息 采购信息 { get; set; }
        public bool 中标商品 { get; set; }
        public List<_中标信息> 中标信息 { get; set; }

        public class _审核数据
        {
            public 审核状态 审核状态 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 审核时间 { get; set; }
            public 用户链接<运营团队> 审核者 { get; set; }
            public string 审核不通过原因 { get; set; }
            public _审核数据() { this.审核者 = new 用户链接<运营团队>(); }
        }
        public class _商品信息
        {
            [Required(ErrorMessage = "请输入商品名")]
            public string 商品名 { get; set; }
            public string 品牌 { get; set; }
            public string 型号 { get; set; }
            public string 精确型号 { get; set; }
            public string 计量单位 { get; set; }
            public List<string> 商品图片 { get; set; }
            public 用户链接<供应商> 所属供应商 { get; set; }
            public 商品分类链接 所属商品分类 { get; set; }

            public _商品信息()
            {
                this.商品图片 = new List<string>();
                this.所属供应商 = new 用户链接<供应商>();
                this.所属商品分类 = new 商品分类链接();
            }
        }
        public class _商品数据
        {
            [Required(ErrorMessage = "请输入商品简介")]
            public string 商品简介 { get; set; }

            [Required(ErrorMessage = "请输入商品详情")]
            public string 商品详情 { get; set; }

            public string 售后服务 { get; set; }
            [BsonDictionaryOptions(DictionaryRepresentation.Document)]
            public Dictionary<string, Dictionary<string, List<string>>> 商品属性 { get; set; }
            public _商品数据()
            {
                this.商品属性 = new Dictionary<string, Dictionary<string, List<string>>>();
            }
        }
        public class _销售信息
        {
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 价格修改日期 { get; set; }

            [Required(ErrorMessage = "请输入商品价格")]
            [RegularExpression(@"^(\d+)(.\d+)?", ErrorMessage = "请输入正确的价格")]
            public decimal 价格 { get; set; }
            public decimal 军采价 { get; set; }
            public int 销量 { get; set; }
            public 库存状态 库存状态 { get; set; }
            public int 点击量 { get; set; }
            public int 内网点击量 { get; set; }
            public _价格属性组合 价格属性组合 { get; set; }
            public _销售信息()
            {
                this.点击量 = RandomString.ClickStartNum();
                this.内网点击量 = RandomString.ClickStartNum();
                this.价格修改日期 = DateTime.Now;
                this.价格属性组合 = new _价格属性组合();
            }
        }
        public enum 库存状态
        {
            未设置 = 0,
            有货 = 1,
            无货 = 2,
        }
        public class _价格属性组合
        {
            public List<string> 属性列表 { get; set; }
            public List<_价格属性搭配> 组合列表 { get; set; }
            public class _价格属性搭配
            {
                public decimal 价格 { get; set; }
                public List<string> 属性值表 { get; set; }
                public int 销量 { get; set; }
                public 库存状态 库存状态 { get; set; }
                public _价格属性搭配() { 属性值表 = new List<string>(); }
            }
            public _价格属性组合()
            {
                this.属性列表 = new List<string>();
                this.组合列表 = new List<_价格属性搭配>();
            }
            public void 设置属性组合列表(params string[] 属性列表)
            {
                this.属性列表 = new List<string>(属性列表);
            }
            public bool 添加组合(decimal 价格, params string[] 属性值)
            {
                if (属性值.Length != this.属性列表.Count) return false;
                this.组合列表.Add(new _价格属性搭配() { 价格 = 价格, 属性值表 = new List<string>(属性值) });
                return true;
            }
        }
        public class _采购信息
        {
            public bool 参与普通采购 { get; set; }
            public bool 参与协议采购 { get; set; }
            public bool 参与应急采购 { get; set; }
        }

        public class _中标信息
        {
            public string 中标项目编号 { get; set; }
            public int 中标数量 { get; set; }
            public decimal 中标金额 { get; set; }

        }
        public 商品()
        {
            this.审核数据 = new _审核数据();
            this.商品信息 = new _商品信息();
            this.商品数据 = new _商品数据();
            this.销售信息 = new _销售信息();
            this.采购信息 = new _采购信息();
            this.中标信息 = new List<_中标信息>();
        }
    }
    public class 商品链接
    {
        public long 商品ID { get; set; }
        public 商品 商品 { get { return 商品管理.查找商品(商品ID); } }
        public 商品链接() { this.商品ID = -1; }
    }
}

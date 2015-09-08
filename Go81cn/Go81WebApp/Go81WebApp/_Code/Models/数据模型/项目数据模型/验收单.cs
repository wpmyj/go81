using System;
using System.Collections.Generic;
using System.Linq;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.管理器;
using System.ComponentModel.DataAnnotations;
using MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Go81WebApp.Models.数据模型.项目数据模型
{
    public class 验收单 : 基本数据模型
    {
        public string 验收单号 { get; set; }
        //public string 验收单二维码 { get; set; }
        public string 验收单核对码 { get; set; }
        //public string 管理单位审核人 { get; set; }
        public 用户链接<供应商> 供应商链接 { get; set; }
        public string 供货单位开票人 { get; set; }
        public string 供货单位开票人电话 { get; set; }
        public 用户链接<单位用户> 管理单位审核人 { get; set; }
        public string 收货单位 { get; set; }
        public string 收货单位验收人 { get; set; }
        public string 收货单位验收人电话 { get; set; }
        [Required(ErrorMessage="请填写验收单位")]
        public List<_物资或服务条目> 物资服务列表 { get; set; }
        public class _物资或服务条目
        {
            public 商品链接 商品链接 { get; set; }
            [Required(ErrorMessage="请填写商品规格型号")]
            public string 规格型号 { get; set; }
            //[Required(ErrorMessage="请填写采购合同号")]
            public string 采购合同号 { get; set; }     
            [Required(ErrorMessage="请填写计量单位")]
            public string 计量单位 { get; set; }
            [Required(ErrorMessage="请填写商品数量")]
            public int 数量 { get; set; }
            [Required(ErrorMessage="请填写商品单价")]
            public decimal 单价 { get; set; }
            public decimal 总价 { get { return this.数量 * this.单价; } }
            public _物资或服务条目() { 商品链接 = new 商品链接(); }
        }
        public decimal 运费 { get; set; }
        public decimal 服务费 { get; set; }
        public decimal 维修费 { get; set; }
        //[BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public List<_其他费用> 其他费用 { get; set; } 
        public 某批物资名称 某批物资名称 { get; set; }
        public _审核数据 审核数据 { get; set; }
        public bool 是否已经打印 { get; set; }
        public List<_打印信息> 打印信息 { get; set; }
        public bool 是否作废 { get; set; }
        public decimal 总金额 { get { return 运费 + 服务费 + 维修费 + 其他费用.Sum(o=>o.金额) + 物资服务列表.Sum(o => o.总价)+验收单附件.Sum(o=>o.价格); } }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] 
        public DateTime 验收时间 { get; set; }
        public List<_回传信息> 验收单扫描件 { get; set; }
        public string 扫描件审核状态 
        { 
            get 
            { 
                if (验收单扫描件.Exists(o => o.审核数据.审核状态 == 审核状态.审核未通过)) { return "审核未通过"; }
                if (验收单扫描件.Exists(o => o.审核数据.审核状态 != 审核状态.审核未通过 && o.审核数据.审核状态 != 审核状态.未审核)) { return "审核通过"; }
                if (验收单扫描件.Exists(o => o.审核数据.审核状态 != 审核状态.审核通过 && o.审核数据.审核状态 != 审核状态.审核未通过)) { return "未审核"; };
                return "审核未通过";
            }
        }
        public int 验收章坐标X { get; set; }
        public int 验收章坐标Y { get; set; } 
        public float 验收章旋转角度 { get; set; }
        public List<_验收单附件> 验收单附件 { get; set; }

        public 验收单()
        {
            var d = DateTime.Now;
            验收单号 = string.Format("{0:D4}{1:D3}{2:D6}", d.Year, d.DayOfYear, Mongo.NextId<验收单>(d.Year.ToString("D:4")));
            Id = long.Parse(验收单号);
            供应商链接 = new 用户链接<供应商>();
            管理单位审核人 = new 用户链接<单位用户>();
            物资服务列表 = new List<_物资或服务条目>();
            审核数据 = new _审核数据();
            验收单附件 = new List<_验收单附件>();
            其他费用 = new List<_其他费用>();
            打印信息 = new List<_打印信息>();
            验收单扫描件 = new List<_回传信息>();
        }
    }

    public class _回传信息
    {
        public string 回传单路径 { get; set; }
        public _审核数据 审核数据 { get; set; }
        public _回传信息()
        {
            审核数据 = new _审核数据();
        }
    }

    public class _打印信息
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 打印时间 { get; set; }
    }
    public class _验收单附件
    {
        public decimal 价格 { get; set; }
        public string 附件路径 { get; set; }
    }
    public class _其他费用
    {
        public string 费用名称 { get; set; }
        public decimal 金额 { get; set; }
    }
    public class 验收单链接
    {
        public long 验收单ID { get; set; }
        public 验收单 验收单 { get { return 验收单管理.查找验收单(验收单ID); } }
        public 验收单链接() { this.验收单ID = -1; }
    }
    public class _审核数据
    {
        public 审核状态 审核状态 { get; set; }
        public 用户链接<单位用户> 审核者 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 审核时间 { get; set; }
        public string 审核不通过原因 { get; set; }
        public _审核数据() { this.审核状态 = 审核状态.未审核; this.审核者 = new 用户链接<单位用户>(); this.审核时间 = DateTime.MinValue; }
    }
    public enum 某批物资名称
    {
        办公物资一批 = 0,
        采购物资一批 = 1,
    }
}
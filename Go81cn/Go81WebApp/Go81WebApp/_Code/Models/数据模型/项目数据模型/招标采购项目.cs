using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.管理器;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型.项目数据模型
{
    public class 招标采购项目 : 基本数据模型
    {
        public 用户链接<单位用户> 需求提报单位 { get; set; }
        public 需求申请链接 需求申请来源 { get; set; }
        public 用户链接<单位用户> 项目管理单位 { get; set; }
        public 项目类型 项目类型 { get; set; }
        public _审核数据 审核数据 { get; set; }
        public class _审核数据
        {
            public 审核状态 审核状态 { get; set; }
            public 用户链接<单位用户> 审核者 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 审核时间 { get; set; }
            public string 审核不通过原因 { get; set; }
            public _审核数据() { this.审核状态 = 审核状态.未审核; this.审核者 = new 用户链接<单位用户>(); this.审核时间 = DateTime.MinValue; }
        }
        public 公告链接 技术公告链接 { get; set; }
        public 公告链接 招标公告链接 { get; set; }
        public 公告链接 中标公告链接 { get; set; }
        public 公告链接 废标公告链接 { get; set; }
        public 公告链接 流标公告链接 { get; set; }
        public List<公告链接> 修正公告链接 { get; set; }
        [Required(ErrorMessage = "项目名称必须填写")]
        public string 项目名称 { get; set; }
        [Required(ErrorMessage = "项目编号必须填写")]
        public string 项目编号 { get; set; }
        [DataType(DataType.Currency, ErrorMessage = "请填写正确的预算金额")]
        public decimal 预算金额 { get; set; }
        public decimal 成交金额 { get; set; }
        public List<_中标结果> 中标结果 { get; set; }
        public class _中标结果
        {
            public 用户链接<供应商> 供应商 { get; set; }
            public List<string> 标的物 { get; set; }
            public _中标结果() { 供应商 = new 用户链接<供应商>(); 标的物 = new List<string>(); }
        }
        public 招标采购项目()
        {
            需求提报单位 = new 用户链接<单位用户>();
            需求申请来源 = new 需求申请链接();
            项目管理单位 = new 用户链接<单位用户>();
            审核数据 = new _审核数据();
            技术公告链接 = new 公告链接();
            招标公告链接 = new 公告链接();
            中标公告链接 = new 公告链接();
            废标公告链接 = new 公告链接();
            流标公告链接 = new 公告链接();
            修正公告链接 = new List<公告链接>();
            中标结果 = new List<_中标结果>();
        }
    }
    public enum 项目类型
    {
        普通 = 0,
        网上竞标 = 1,
    }
    public class 招标采购项目链接
    {
        public long 招标采购项目ID { get; set; }
        public 招标采购项目 招标采购项目 { get { return 招标采购项目管理.查找招标采购项目(招标采购项目ID); } }
        public 招标采购项目链接() { this.招标采购项目ID = -1; }
    }
}
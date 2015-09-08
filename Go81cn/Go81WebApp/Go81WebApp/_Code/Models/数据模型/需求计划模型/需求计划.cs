using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Description;
using Go81WebApp.Models.数据模型.审核数据结构;
using MongoDB.Bson.Serialization.Attributes;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.管理器.需求计划管理;

namespace Go81WebApp.Models.数据模型.需求计划模型
{    
    public class 需求计划 : 基本数据模型
    {
        public 用户链接<单位用户> 当前处理单位链接 { get; set; }
        public List<用户链接<单位用户>> 审批流程单位列表 { get; set; }
        public 用户链接<单位用户> 需求发起单位链接 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 提报日期 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 建议完成时间 { get; set; }
        public string 需求计划标题 { get; set; }
        public string 需求计划主标题 { get; set; }
        public 秘密等级 秘密等级 { get; set; }
        public string 编制单位 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 采购年度 { get; set; }
        public string 承办部门 { get; set; }
        public 需求计划链接 并入需求计划链接 { get; set; }
        public List<需求计划链接> 来源需求计划列表 { get; set; }
        public List<需求计划物资链接> 物资列表 { get; set; }
        public List<需求计划分发链接> 分发列表 { get; set; }
        public List<_审核数据> 审核历史列表 { get; set; }
        public bool 流程已完成 { get; set; }
        public bool 已下达 { get; set; }
        public string 联系人 { get; set; }
        public string 联系电话 { get; set; }
        public class _审核数据
        {
            public 审核状态 审核状态 { get; set; }
            public 用户链接<用户基本数据> 审核者 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 审核时间 { get; set; }
            public string 审核不通过原因 { get; set; }
            public List<需求计划物资链接> 未通过审批物资列表 { get; set; }
            public _审核数据()
            {
                审核状态 = 审核状态.未审核;
                审核者 = new 用户链接<用户基本数据>();
                审核时间 = DateTime.MinValue;
                未通过审批物资列表 = new List<需求计划物资链接>();
            }
        }
        public 需求计划()
        {
            当前处理单位链接 = new 用户链接<单位用户>();
            审批流程单位列表 = new List<用户链接<单位用户>>();
            需求发起单位链接 = new 用户链接<单位用户>();
            并入需求计划链接 = new 需求计划链接();
            来源需求计划列表 = new List<需求计划链接>();
            物资列表 = new List<需求计划物资链接>();
            分发列表 = new List<需求计划分发链接>();
            审核历史列表 = new List<_审核数据>();
        }
    }
    public class 需求计划物资 : 基本数据模型
    {
        public 需求计划链接 所属需求计划 { get; set; }
        public List<需求计划物资链接> 来源合并项 { get; set; }
        public int 物资编号 { get; set; }
        public string 物资名称 { get; set; }
        public string 规格型号 { get; set; }
        public string 计量单位 { get; set; }
        public decimal 数量 { get; set; }
        public decimal 单价 { get; set; }
        public decimal 预算金额 { get; set; }
        public string 技术指标 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 交货期限 { get; set; }
        public string 建议采购方式 { get; set; }
        public string 供应商名称 { get; set; }
        public string 备注 { get; set; }
        public bool 已下达 { get; set; }
        public 需求计划物资()
        {
            所属需求计划 = new 需求计划链接();
            来源合并项 = new List<需求计划物资链接>();
        }
    }
    public enum 采购方式
    {
        未设置 = 0,
        公开招标 = 1,
        邀请招标 = 2,
        询价采购 = 3,
        竞争性谈判 = 4,
        单一来源 = 5,
        协议采购 = 6,
        网上竞标 = 7,
        其他 = 9999,
    }
    public enum 秘密等级
    {
        未设置 = 0,
        秘密 = 1,
        机密 = 2,
        绝密 = 3
    }
    public class 需求计划分发 : 基本数据模型
    {
        public 需求计划链接 所属需求计划 { get; set; }
        public string 收货单位名称 { get; set; }
        public int 分发编号 { get; set; }
        public string 物资名称 { get; set; }
        public string 规格型号 { get; set; }
        public string 计量单位 { get; set; }
        public decimal 分配数量 { get; set; }
        public 提货方式 提货方式 { get; set; }
        public 运输方式 运输方式 { get; set; }
        public string 到站 { get; set; }
        public string 备注 { get; set; }
        public bool 已下达 { get; set; }
        public 需求计划分发()
        {
            所属需求计划 = new 需求计划链接();
        }
    }
    public enum 提货方式
    {
        未设置 = 0,
        自提 = 1,
        代运 = 2,
    }
    public enum 运输方式
    {
        未设置 = 0,
        铁路运输 = 1,
        公路运输 = 2,
        水路运输 = 3,
        航空运输 = 4,
    }
    public class 需求计划链接
    {
        public long 需求计划ID { get; set; }
        public 需求计划 需求计划数据 { get { return 需求计划管理.查找需求计划(需求计划ID); } }
        public 需求计划链接() { 需求计划ID = -1; }
    }
    public class 需求计划物资链接
    {
        public long 需求计划物资ID { get; set; }
        public 需求计划物资 需求计划物资数据 { get { return 需求计划物资管理.查找需求计划物资(需求计划物资ID); } }
        public 需求计划物资链接() { 需求计划物资ID = -1; }
    }
    public class 需求计划分发链接
    {
        public long 需求计划分发ID { get; set; }
        public 需求计划分发 需求计划分发数据 { get { return 需求计划分发管理.查找需求计划分发(需求计划分发ID); } }
        public 需求计划分发链接() { 需求计划分发ID = -1; }
    }

    public class 需求采购任务 : 需求计划
    {
         public 采购方式 采购方式 { get; set; }
         public string 描述 { get; set; }
         public 用户链接<单位用户> 受理单位 { get; set; }

        public 需求采购任务()
        {
            this.受理单位 = new 用户链接<单位用户>();
        }
    }
}
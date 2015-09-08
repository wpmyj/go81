using System;
using System.Collections.Generic;
using System.Linq;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Go81WebApp.Models.数据模型.推广业务数据模型
{
    public class 供应商充值余额 : 基本数据模型
    {
        public 用户链接<供应商> 所属供应商 { get; set; }
        public decimal 可用余额 { get; set; }
        public 供应商充值余额()
        {
            所属供应商 = new 用户链接<供应商>();
        }
    }
    public class 订购合同上传记录 : 基本数据模型
    {
        public 用户链接<供应商> 所属供应商 { get; set; }
        public 订购合同 合同 { get; set; }
        public 订购合同上传记录()
        {
            所属供应商 = new 用户链接<供应商>();
        }
    }
    public class 订购合同
    {
        public string 合同名称 { get; set; }
        public Dictionary<string, int> 包含服务 { get; set; }
        public List<string> 合同路径 { get; set; }
        public 订购合同()
        {
            包含服务 = new Dictionary<string, int>();
            合同路径 = new List<string>();
        }
    }
    public class 供应商充值记录 : 基本数据模型
    {
        public 用户链接<供应商> 所属供应商 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 充值时间 { get; set; }
        public 充值方式 充值方式 { get; set; }
        public string 供应商转款账号 { get; set; }
        public string 收款账号 { get; set; }
        public decimal 充值金额 { get; set; }
        public 供应商充值记录()
        {
            所属供应商 = new 用户链接<供应商>();
        }
    }
    public enum 充值方式
    {
        后台直充 = 0,
        支付宝 = 1,
        银行转账 = 2,
    }
    public class 供应商计费项目扣费记录 : 基本数据模型
    {
        public 用户链接<供应商> 所属供应商 { get; set; }
        public string 扣费服务项目 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 扣费时间 { get; set; }
        public decimal 扣费金额 { get; set; }
        public 扣费类型 扣费类型 { get; set; }
        public int 所属年 { get; set; }
        public int 所属月 { get; set; }
        public int 所属日 { get; set; }
        public 审核状态 审核数据 { get; set; }
        public 供应商计费项目扣费记录()
        {
            所属供应商 = new 用户链接<供应商>();
        }
    }

    public class 供应商增值服务申请记录 : 基本数据模型
    {
        public 用户链接<供应商> 所属供应商 { get; set; }
        public string 所申请项目名 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 签订时间 { get; set; }
        public int 服务期限 { get; set; }
        public int 开通个数 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 结束时间 { get; set; }
        public 通过状态 是否通过 { get; set; }
        public string 未通过原因 { get; set; }
        public 供应商增值服务申请记录()
        {
            所属供应商 = new 用户链接<供应商>();
        }
    }

    public class 供应商服务记录 : 基本数据模型
    {
        public 用户链接<供应商> 所属供应商 { get; set; }
        public List<供应商增值服务申请记录> 申请中的服务 { get; set; }
        public List<供应商增值服务申请记录> 可申请的服务 { get; set; }
        public List<供应商增值服务申请记录> 已开通的服务 { get; set; }
        public 供应商服务记录()
        {
            所属供应商 = new 用户链接<供应商>();
            申请中的服务 = new List<供应商增值服务申请记录>();
            可申请的服务 = new List<供应商增值服务申请记录>();
            已开通的服务 = new List<供应商增值服务申请记录>();
        }
    }
    public enum 扣费类型
    {
        按月扣费 = 0,
        按年扣费 = 1,
        按次扣费 = 2,
    }
    public enum 通过状态
    {
        未通过,
        通过
    }
    public class 扣费规则
    {
        public string 扣费项目名 { get; set; }
        public string 子类型 { get; set; }
        public int 附加数据 { get; set; }
        public 扣费类型 扣费类型 { get; set; }
        public decimal 扣费金额 { get; set; }
        public static readonly 扣费规则[] 规则列表 =
        {
            new 扣费规则 {扣费项目名 = "基础会员", 扣费类型 = 扣费类型.按年扣费, 扣费金额 =360},
            new 扣费规则 {扣费项目名 = "标准会员", 扣费类型 = 扣费类型.按年扣费, 扣费金额 =688},
            new 扣费规则 {扣费项目名 = "商务会员", 扣费类型 = 扣费类型.按年扣费, 扣费金额 =1288},
            new 扣费规则 {扣费项目名 = "手机短信招标采购类", 扣费类型 = 扣费类型.按月扣费, 扣费金额 =15},
            new 扣费规则 {扣费项目名 = "手机短信动态与提醒", 扣费类型 = 扣费类型.按月扣费, 扣费金额 =15},
            new 扣费规则 {扣费项目名 = "手机短信业务对接", 扣费类型 = 扣费类型.按月扣费, 扣费金额 =15},
            new 扣费规则 {扣费项目名 = "添加商品一级分类", 扣费类型 = 扣费类型.按年扣费, 扣费金额 =180},
            new 扣费规则 {扣费项目名 = "添加商品二级分类", 扣费类型 = 扣费类型.按年扣费, 扣费金额 =40},
            new 扣费规则 {扣费项目名 = "批量上传商品", 扣费类型 = 扣费类型.按月扣费, 扣费金额 =10},
            new 扣费规则 {扣费项目名 = "企业主页商品展示A类窗口",附加数据=Int32.MaxValue, 扣费类型 = 扣费类型.按月扣费, 扣费金额 =60},
            new 扣费规则 {扣费项目名 = "企业主页商品展示B类窗口",附加数据=200, 扣费类型 = 扣费类型.按月扣费, 扣费金额 =45},
            new 扣费规则 {扣费项目名 = "企业主页商品展示C类窗口",附加数据=80, 扣费类型 = 扣费类型.按月扣费, 扣费金额 =30},
            new 扣费规则 {扣费项目名 = "企业主页商品展示D类窗口",附加数据=60, 扣费类型 = 扣费类型.按月扣费, 扣费金额 =25},
            new 扣费规则 {扣费项目名 = "企业主页商品展示E类窗口",附加数据=40, 扣费类型 = 扣费类型.按月扣费, 扣费金额 =20},
            new 扣费规则 {扣费项目名 = "企业主页商品展示F类窗口",附加数据=20, 扣费类型 = 扣费类型.按月扣费, 扣费金额 =15},
            new 扣费规则 {扣费项目名 = "一对一客服服务",扣费类型 = 扣费类型.按月扣费, 扣费金额 =0},
            new 扣费规则 {扣费项目名 = "U盾服务",扣费类型 = 扣费类型.按年扣费, 扣费金额 =360},
            new 扣费规则 {扣费项目名 = "购买U盾",扣费类型 = 扣费类型.按次扣费, 扣费金额 =180},
            new 扣费规则 {扣费项目名 = "本网加信用认证标识",扣费类型 = 扣费类型.按年扣费, 扣费金额 =600},
            new 扣费规则 {扣费项目名 = "第三方信用查询",扣费类型 = 扣费类型.按年扣费, 扣费金额 =3500},
            new 扣费规则 {扣费项目名 = "信用认证服务",扣费类型 = 扣费类型.按年扣费, 扣费金额 =800},
            new 扣费规则 {扣费项目名 = "出具供应商信用数据",扣费类型 = 扣费类型.按次扣费, 扣费金额 =3500},
            new 扣费规则{扣费项目名 = "第三方信用认证服务",扣费类型 = 扣费类型.按年扣费, 扣费金额 =0},
            new 扣费规则{扣费项目名 = "内网商品点击率查询",扣费类型 = 扣费类型.按年扣费, 扣费金额 =0},
            new 扣费规则{扣费项目名 = "搜索栏",扣费类型 = 扣费类型.按次扣费, 扣费金额 =0},
            new 扣费规则{扣费项目名 = "企业推广服务A1位置",扣费类型 = 扣费类型.按月扣费, 扣费金额 =4000},
            new 扣费规则{扣费项目名 = "企业推广服务A2位置",扣费类型 = 扣费类型.按月扣费, 扣费金额 =3000},
            new 扣费规则{扣费项目名 = "企业推广服务A3位置",扣费类型 = 扣费类型.按月扣费, 扣费金额 =2000},
            new 扣费规则{扣费项目名 = "企业推广服务B1-1位置",扣费类型 = 扣费类型.按月扣费, 扣费金额 =1500},
            new 扣费规则{扣费项目名 = "企业推广服务B1-2位置",扣费类型 = 扣费类型.按月扣费, 扣费金额 =3000},
            new 扣费规则{扣费项目名 = "企业推广服务B2位置",扣费类型 = 扣费类型.按月扣费, 扣费金额 =2000},
            new 扣费规则{扣费项目名 = "企业推广服务B3位置",扣费类型 = 扣费类型.按月扣费, 扣费金额 =3000},
            new 扣费规则{扣费项目名 = "企业推广服务C1位置",扣费类型 = 扣费类型.按月扣费, 扣费金额 =3000},
            new 扣费规则{扣费项目名 = "企业推广服务C2位置",扣费类型 = 扣费类型.按月扣费, 扣费金额 =2000},
        };
    }
}
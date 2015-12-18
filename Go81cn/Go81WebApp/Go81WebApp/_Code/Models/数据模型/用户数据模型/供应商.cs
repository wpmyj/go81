using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.项目数据模型;
using Go81WebApp.Models.管理器;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Helpers;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp._Code.Models.数据模型.用户数据模型;

namespace Go81WebApp.Models.数据模型.用户数据模型
{
    public class 供应商 : 用户基本数据
    {
        private static readonly Dictionary<认证级别, int> 预设固定订阅数 = new Dictionary<认证级别, int>() {
            { 认证级别.未设置, 0 },
            { 认证级别.一级供应商, 2 },
            { 认证级别.二级供应商, 3 },
            { 认证级别.军采通基础会员, 4 },
            { 认证级别.军采通标准会员, 5 },
            { 认证级别.军采通商务会员, 6 },
        };
        public _审核数据 审核数据 { get; set; }
        public class _审核数据
        {
            public 审核状态 审核状态 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 审核时间 { get; set; }
            public 用户链接<用户基本数据> 审核者 { get; set; }
            public string 审核不通过原因 { get; set; }
            public _审核数据() { this.审核者 = new 用户链接<用户基本数据>(); }
        }
        public _供应商用户信息 供应商用户信息 { get; set; }
        public class _供应商用户信息
        {
            public bool 已填写完整 { get; set; }
            public bool 已提交 { get; set; }
            public 入库级别 入库级别 { get; set; }
            public 采购管理单位 所属管理单位 { get; set; }
            public string 供应商显示名 { get; set; }
            public List<string> 供应商图片 { get; set; }
            public string 展示图片 { get; set; }
            public string 供应商编码 { get; set; }
            public 认证级别 认证级别 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 军采通开通时间 { get; set; }
            [BsonDictionaryOptions(DictionaryRepresentation.Document)]
            public Dictionary<string, 操作数据> 认证数据 { get; set; }
            public bool 普通供应商 { get; set; }
            public bool 协议供应商 { get; set; }
            public List<_地域> 协议供应商所属地区 { get; set; }
            public 供应商来源 用户来源 { get; set; }
            public bool 应急供应商 { get; set; }
            public 供应商类型 供应商类型 { get; set; }
            public 供应商细分类型 供应商细分类型 { get; set; }
            public List<string> 实地照片 { get; set; }
            [BsonDictionaryOptions(DictionaryRepresentation.Document)]
            public Dictionary<string, 操作数据> 年检列表 { get; set; }
            public int 点击量 { get; set; }
            public int 内网点击量 { get; set; }
            public List<商品链接> 展示商品 { get; set; }

            public _审核数据 初审数据 { get; set; }
            public _审核数据 复审数据 { get; set; }
            public _U盾信息 U盾信息 { get; set; }
            public _信用等级信息 信用等级信息 { get; set; }
            public bool 符合入库标准 { get; set; }
            public Dictionary<string, List<_广告商品>> 广告商品 { get; set; }
            public class _信用等级信息
            {
                public 信用等级 信用等级 { get; set; }
                public string 信用等级评定机构 { get; set; }
                [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
                public DateTime 评定时间 { get; set; }
            }
            public class _广告商品
            {
                public 商品链接 商品;
                public int 序号 { get; set; }

                public _广告商品()
                {
                    商品 = new 商品链接();
                }
            }
            public class _U盾信息
            {
                public string 序列号;
                public string 加密参数;
                [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
                public DateTime 年检开始时间;
                [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
                public DateTime 年检结束时间;
                public string 身份认证申请表;
                public string 办理人授权书;
                public string 办理人身份证照片;
                public string U盾操作人授权书;
                public string U盾操作人身份证照片;
                public string 办公场地照片;
                public List<string> 安全认证年度审核申请表;
                public _审核数据 审核数据;
                public _U盾信息()
                {
                    this.安全认证年度审核申请表 = new List<string>();
                    this.审核数据 = new _审核数据();
                }
            }
            public _供应商用户信息()
            {
                this.供应商图片 = new List<string>();
                this.协议供应商所属地区 = new List<_地域>();
                this.认证数据 = new Dictionary<string, 操作数据>();
                this.实地照片 = new List<string>();
                this.普通供应商 = true;
                this.年检列表 = new Dictionary<string, 操作数据>();
                this.点击量 = RandomString.ClickStartNum();
                this.内网点击量 = RandomString.ClickStartNum();
                this.展示商品 = new List<商品链接>();
                this.初审数据 = new _审核数据();
                this.复审数据 = new _审核数据();
                this.U盾信息 = new _U盾信息();
                this.信用等级信息 = new _信用等级信息();
                this.广告商品 = new Dictionary<string, List<_广告商品>>();
            }
        }

        public enum 入库级别
        {
            未设置 = 0,
            入网供应商 = 1,
            成都军区库 = 2,
            全军库 = 3,
        }
        public enum 供应商来源
        {
            未设置 = 0,
            主动申请 = 1,
            单位推荐 = 2,
        }
        public enum 供应商类型
        {
            物资供应商 = 0,
            服务供应商 = 1,
            工程供应商 = 2,
        }
        public enum 供应商细分类型
        {
            未填写 = 0,
            酒店 = 11,
            机票代售点 = 12,
            租车企业 = 13,
        }
        public enum 采购管理单位
        {
            未选择,
            成都军区物资采购机构_成都,
            成都军区物资采购机构_昆明,
            成都军区物资采购机构_贵阳,
            成都军区物资采购机构_重庆,
            西藏军区物资采购中心,
        }
        public enum 认证级别
        {
            未设置 = 0,
            一级供应商 = 2,
            二级供应商 = 3,
            军采通基础会员 = 4,
            军采通标准会员 = 5,
            军采通商务会员 = 6,
        }
        public enum 信用等级
        {
            未设置 = 0,
            AAA级 = 1,
            AA级 = 2,
            A级 = 3,
            BBB级 = 4,
            BB级 = 5,
            B级 = 6,
            CCC级 = 7,
            CC级 = 8,
            C级 = 9,
            D级 = 10,
        }
        public List<_历史参标记录> 历史参标记录 { get; set; }
        public class _历史参标记录
        {
            [Required(ErrorMessage = "请确认是否是本平台项目")]
            public bool 本平台项目 { get; set; }
            [Required(ErrorMessage = "请填写项目名称")]
            public string 项目名称 { get; set; }
            [Required(ErrorMessage = "请填写招标方")]
            public string 招标方 { get; set; }
            [Required(ErrorMessage = "请填写招标结果")]
            public 参标结果 参标结果 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            [Required(ErrorMessage = "请填写参标时间")]
            public DateTime 参标时间 { get; set; }
        }
        public enum 参标结果
        {
            未填写 = 0,
            中标 = 1,
            落标 = 2,
            流标 = 3,
            废标 = 4,
            其他 = 9999
        }
        public int 固定可订阅数 { get { return 预设固定订阅数[供应商用户信息.认证级别]; } }
        public int 已购买订阅数 { get; set; }
        public int 总计可订阅数 { get { return 固定可订阅数 + 已购买订阅数; } }
        public _消息订阅信息 消息订阅信息 { get; set; }
        public class _消息订阅信息
        {
            [Required(ErrorMessage = "*请填写手机号码")]
            [RegularExpression("^1[345689][0-9]{9}", ErrorMessage = "*请正确输入手机号")]
            public string 接收订阅短信手机号码 { get; set; }
            [Required(ErrorMessage = "*请填写邮箱号")]
            [RegularExpression("[a-zA-Z0-9]{1,}[@]{1}[A-Za-z]{1,}(.com)$", ErrorMessage = "*请正确输入邮箱号")]
            public string 接收订阅邮件地址 { get; set; }
            [Required(ErrorMessage = "*请填写微信号")]
            [RegularExpression("([a-zA-Z0-9]{1,}[@]{1}[A-Za-z]{1,}(.com)$)|[0-9A-Za-z]{1,}", ErrorMessage = "*请正确输入微信号")]
            public string 接收订阅微信账号 { get; set; }
            public List<_订阅细节> 订阅列表 { get; set; }
            public _消息订阅信息() { this.订阅列表 = new List<_订阅细节>(); }
        }
        public class _订阅细节
        {
            public bool 手机短信 { get; set; }
            public bool 微信推送 { get; set; }
            public bool 电子邮件 { get; set; }
            public bool 站内消息 { get; set; }
            [Required(ErrorMessage = "*请填写一级分类")]
            public string 一级分类 { get; set; }
            [Required(ErrorMessage = "*请填写二级分类")]
            public string 二级分类 { get; set; }
            [Required(ErrorMessage = "*请填写三级分类")]
            public string 三级分类 { get; set; }
        }
        public _企业基本信息 企业基本信息 { get; set; }
        public class _企业基本信息
        {
            public bool 已填写完整 { get; set; }
            [Required(ErrorMessage = "*必填")]
            public string 企业名称 { get; set; }
            public string 英文名称 { get; set; }
            public string 简称 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 成立时间 { get; set; }
            [Required(ErrorMessage = "*必填")]
            public string 所属行业 { get; set; }
            [Required(ErrorMessage = "*必填")]
            public string 所属行业二级分类 { get; set; }
            public string 地图坐标 { get; set; }
            public 经营类型 经营类型 { get; set; }
            public 供应商类别 供应商类别 { get; set; }
            public 经营子类型 经营子类型 { get; set; }
            public 经济性质 经济性质 { get; set; }
            public 员工人数 员工人数 { get; set; }
            public string 企业简介 { get; set; }
            public string 网站网址 { get; set; }
            public string 参加投标经历 { get; set; }
            public string 注册地址 { get; set; }
            [RegularExpression("[0-9]{6}", ErrorMessage = "*请输入正确的邮政编码")]
            public string 邮政编码 { get; set; }
        }
        public enum 经营类型
        {
            未填写 = 0,
            生产型 = 1,
            销售型 = 2,
            服务型 = 3,
        }
        public enum 供应商类别
        {
            未选择 = 0,
            A = 1,
            B = 2,
            C = 3,
            其他 = 4,
        }
        public enum 经营子类型
        {
            未填写 = 0,
            代理 = 1,
            经销 = 2,
            批发零售 = 3,
            系统集成 = 4,
            设备安装 = 5,
            物流 = 6,
            安全检测 = 7,
        }
        public enum 经济性质
        {
            未设置 = 0,
            国有企业 = 1,
            集体企业 = 2,
            私有企业 = 3,
            有限责任企业 = 4,
            股份有限企业 = 5,
        }
        public enum 员工人数
        {
            未设置 = 0,
            人数1至9 = 1,
            人数10至49 = 2,
            人数50至99 = 3,
            人数100至199 = 4,
            人数200至499 = 5,
            人数500至999 = 6,
            人数1000以上 = 7,
        }
        public _企业联系人信息 企业联系人信息 { get; set; }
        public class _企业联系人信息
        {
            public bool 已填写完整 { get; set; }
            public string 联系人姓名 { get; set; }
            public 性别 联系人性别 { get; set; }
            [RegularExpression(@"^(13|14|15|16|18|19)\d{9}$", ErrorMessage = "*请输入正确的手机格式")]
            public string 联系人手机 { get; set; }

            [RegularExpression(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "*请输入正确的邮箱")]
            public string 联系人邮箱 { get; set; }

            [RegularExpression(@"\d{2,5}-\d{7,8}|^(13|14|15|16|18|19)\d{9}$", ErrorMessage = "*正确格式为：区号-电话号码")]
            public string 联系人固定电话 { get; set; }
            public string 联系人传真 { get; set; }
            public string 联系人地址 { get; set; }
        }
        public _工商注册信息 工商注册信息 { get; set; }
        public class _工商注册信息
        {
            public bool 已填写完整 { get; set; }
            public string 组织机构代码 { get; set; }
            public string 组织机构代码证电子扫描件 { get; set; }
            public string 基本账户开户银行 { get; set; }
            [RegularExpression("[0-9,a-z,A-Z]{9,}", ErrorMessage = "请输入正确的银行账号")]
            public string 基本账户银行帐号 { get; set; }
            public bool 基本账户开户银行资信证明 { get; set; }
            public string 基本账户开户银行资信证明电子扫描件 { get; set; }
            public bool 近3年缴纳社会保证金证明 { get; set; }
            public string 近3年缴纳社会保证金证明电子扫描件 { get; set; }
            public bool 近3年有无重大违法记录 { get; set; }
            public string 近3年有无重大违法记录电子扫描件 { get; set; }
        }
        public _营业执照信息 营业执照信息 { get; set; }
        public class _营业执照信息
        {
            public bool 已填写完整 { get; set; }
            public string 营业执照电子扫描件 { get; set; }
            public string 三证合一扫描件 { get; set; }//营业执照，组织机构代码和税务登记证
            public string 营业执照发证机关 { get; set; }
            public string 营业执照注册号 { get; set; }
            public string 税务登记证电子扫描件 { get; set; }
            public string 营业执照注册地址 { get; set; }
            [DataType(DataType.Currency, ErrorMessage = "请填写正确的注册资金")]
            public decimal 营业执照注册资金 { get; set; }
            public 有效期类型 营业执照有效期类型 { get; set; }
            [DataType(DataType.Date)]
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 营业执照有效期起始日期 { get; set; }
            [DataType(DataType.Date)]
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 营业执照有效期结束日期 { get; set; }
            [DataType(DataType.Date)]
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 营业执照最近年检日期 { get; set; }
            public string 经营范围 { get; set; }
            public string 兼营范围 { get; set; }

        }
        public _法定代表人信息 法定代表人信息 { get; set; }
        public class _法定代表人信息
        {
            public bool 已填写完整 { get; set; }
            public string 法定代表人姓名 { get; set; }
            [RegularExpression(@"[0-9]{18}|[0-9]{17}X|[A-Z,a-z]{1,}[0-9]{6,9}|[A-Z,a-z]{1,}[0-9]{6}\D[0-9]{1}\D", ErrorMessage = "*请输入正确的身份证号码")]
            public string 法定代表人身份证号 { get; set; }
            public string 法定代表人身份证电子扫描件 { get; set; }
            [StringLength(11, ErrorMessage = "请输入11位的手机号码")]
            [RegularExpression("^1[345689]{1}[0-9]{9}", ErrorMessage = "*请输入正确的手机号码")]
            public string 法定代表人手机 { get; set; }
            [RegularExpression(@"\d{2,5}-\d{7,8}", ErrorMessage = "*正确格式为：区号-电话号码")]
            public string 法定代表人固定电话 { get; set; }
        }
        public List<_出资人或股东信息> 出资人或股东信息 { get; set; }
        public class _出资人或股东信息
        {
            public bool 已填写完整 { get; set; }
            public string 出资人或股东性质 { get; set; }
            public string 国籍 { get; set; }
            public string 出资人或股东姓名 { get; set; }
            [RegularExpression("[0-9]{17}[0-9,X]{1}", ErrorMessage = "*请正确输入您的18位有效身份证号码")]
            public string 身份证号码 { get; set; }
            [DataType(DataType.Currency, ErrorMessage = "*必需")]
            public decimal 出资金额 { get; set; }
            public double 所占比例 { get; set; }
            public DateTime 出资时间 { get; set; }
        }
        public List<_财务状况信息> 财务状况信息 { get; set; }
        public class _财务状况信息
        {
            public bool 已填写完整 { get; set; }
            public int 年份 { get; set; }
            public string 会计事务所名称 { get; set; }
            [RegularExpression(@"\d{2,5}-\d{7,8}|(^1[345689]{1}[0-9]{9})", ErrorMessage = "*请输入正确的号码")]
            public string 会计事务所联系电话 { get; set; }
            public bool 会计报表是否经注册会计师审计 { get; set; }
            [DataType(DataType.Currency, ErrorMessage = "*请输入正确的资金")]
            public decimal 资产总额 { get; set; }
            [DataType(DataType.Currency, ErrorMessage = "*请输入正确的资金")]
            public decimal 负债总额 { get; set; }
            [DataType(DataType.Currency, ErrorMessage = "*请输入正确的资金")]
            public decimal 销售收入 { get; set; }
            [DataType(DataType.Currency, ErrorMessage = "*请输入正确数字")]
            public double 资产负债率 { get; set; }
            public double 利润总额 { get; set; }
            [DataType(DataType.Currency, ErrorMessage = "*请输入正确的资金")]
            public decimal 净利润 { get; set; }
            public decimal 资本金 { get; set; }
        }
        public _税务信息 税务信息 { get; set; }
        public class _税务信息
        {
            public bool 已填写完整 { get; set; }
            public string 国税税务登记证号 { get; set; }
            public string 国税税务登记证发证机关 { get; set; }
            public 有效期类型 国税税务登记证有效期类型 { get; set; }
            [DataType(DataType.Date, ErrorMessage = "*请输入日期")]
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 国税税务登记证有效期起始日期 { get; set; }
            [DataType(DataType.Date, ErrorMessage = "*请输入日期")]
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 国税税务登记证有效期结束日期 { get; set; }
            public string 地税税务登记证号 { get; set; }
            public string 地税税务登记证发证机关 { get; set; }
            public 有效期类型 地税税务登记证有效期类型 { get; set; }
            [DataType(DataType.Date, ErrorMessage = "*请输入日期")]
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 地税税务登记证有效期起始日期 { get; set; }
            [DataType(DataType.Date, ErrorMessage = "*请输入日期")]
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 地税税务登记证有效期结束日期 { get; set; }
            public bool 近3年完税证明 { get; set; }
            public string 近3年完税证明电子扫描件 { get; set; }
        }
        public enum 有效期类型
        {
            未填写 = 0,
            长期有效 = 1,
            具体时间 = 2,
        }
        public List<_产品类别> 可提供产品类别列表 { get; set; }
        public class _产品类别
        {
            public string 一级分类 { get; set; }
            public List<string> 二级分类 { get; set; }

            public _产品类别()
            {
                二级分类 = new List<string>();
            }
        }
        public List<_资质证书信息> 资质证书列表 { get; set; }
        public class _资质证书信息
        {
            public bool 已填写完整 { get; set; }
            public string 名称 { get; set; }
            public string 等级 { get; set; }
            public List<_电子扫描件> 资质证书电子扫描件 { get; set; }
            public string 发证机构 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 有效期起始日期 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 有效期结束日期 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 年检日期 { get; set; }
            public _资质证书信息() { 资质证书电子扫描件 = new List<_电子扫描件>(); }
        }
        public class _电子扫描件
        {
            public string 名称 { get; set; }
            public string 说明 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 上传日期 { get; set; }
            public string 路径 { get; set; }
        }
        public List<_售后服务机构信息> 售后服务机构列表 { get; set; }
        public class _售后服务机构信息
        {
            public bool 已填写完整 { get; set; }
            public string 售后服务机构名称 { get; set; }
            public 售后服务机构类别 售后服务机构类别 { get; set; }
            public _地域 所在地 { get; set; }
            public _联系方式 负责人联系方式 { get; set; }
        }
        public enum 售后服务机构类别
        {
            未指定 = 0,
            售后服务机构 = 1,
        }
        public _信用评级信息 信用评级信息 { get; set; }
        public class _信用评级信息
        {
            public List<_供应商评价> 供应商评价 { get; set; }
            public int 积分 { get; set; }
            public int 等级 { get; set; }
            public _信用评级信息() { 供应商评价 = new List<_供应商评价>(); }
        }
        public class _供应商评价
        {
            public string 项目概要 { get; set; }
            public 招标采购项目链接 所属招标采购项目 { get; set; }
            public int 中标名次 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 中标时间 { get; set; }
        }
        public 供应商()
        {
            审核数据 = new _审核数据();
            供应商用户信息 = new _供应商用户信息();
            历史参标记录 = new List<_历史参标记录>();
            消息订阅信息 = new _消息订阅信息();
            企业基本信息 = new _企业基本信息();
            企业联系人信息 = new _企业联系人信息();
            工商注册信息 = new _工商注册信息();
            营业执照信息 = new _营业执照信息();
            法定代表人信息 = new _法定代表人信息();
            出资人或股东信息 = new List<_出资人或股东信息>();
            财务状况信息 = new List<_财务状况信息>();
            税务信息 = new _税务信息();
            可提供产品类别列表 = new List<_产品类别>();
            资质证书列表 = new List<_资质证书信息>();
            售后服务机构列表 = new List<_售后服务机构信息>();
            信用评级信息 = new _信用评级信息();
            历史投标补充资料 = new Dictionary<string, List<string>>();
        }
        public bool 修改认证级别(认证级别 认证级别)
        {
            if (1 != 用户管理.更新用户<供应商>(
                Query.EQ("_id", Id),
                Update.Set("供应商用户信息.认证级别", 认证级别)
                )) return false;
            用户管理.用户更新通知<供应商>(Id);
            return true;
        }
        public bool 更新订阅信息(_消息订阅信息 订阅信息)
        {
            if (1 != 用户管理.更新用户<供应商>(
                Query.EQ("_id", Id),
                Update.Set("消息订阅信息", 订阅信息.ToBsonDocument())
                )) return false;
            用户管理.用户更新通知<供应商>(Id);
            return true;
        }
        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, List<string>> 历史投标补充资料 { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Lucene.Net.Search;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver.Builders;

namespace Go81WebApp.Models.数据模型.用户数据模型
{
    public class 专家 : 用户基本数据
    {
        public 采购管理单位 所属管理单位 { get; set; }
        public _审核数据 审核数据 { get; set; }
        public class _审核数据
        {
            public 审核状态 审核状态 { get; set; }
            public 用户链接<用户基本数据> 审核者 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 审核时间 { get; set; }
            public string 审核不通过原因 { get; set; }
            public _审核数据() { this.审核状态 = 审核状态.未审核; this.审核者 = new 用户链接<用户基本数据>(); this.审核时间 = DateTime.MinValue; }
        }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        //public DateTime 上次出席评标时间 { get; set; }
        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, List<DateTime>> 历史抽取信息 { get; set; }
        public _身份信息 身份信息 { get; set; }
        public class _身份信息
        {
            public 专家类型 专家类型 { get; set; }
            [Required(ErrorMessage = "请填写姓名！")]
            public string 姓名 { get; set; }
            public 性别 性别 { get; set; }
            public 民族 民族 { get; set; }
            public 政治面貌 政治面貌 { get; set; }
            [Required(ErrorMessage = "请填写出生年月！")]
            [DataType(DataType.Date)]
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 出生年月 { get; set; }
            public 证件类型 证件类型 { get; set; }
            [Required(ErrorMessage = "请填写证件号！")]
            public string 证件号 { get; set; }
            [Required(ErrorMessage = "请上传扫描件！")]
            public string 证件电子扫描件 { get; set; }
            [Required(ErrorMessage = "请上传照片！")]
            public string 本人照片电子扫描件 { get; set; }
            public 专家类别 专家类别 { get; set; }
            public 专家级别 专家级别 { get; set; }
            public string 专家证号 { get; set; }
            public List<string> 专家证电子扫描件 { get; set; }
            public bool 评标经历 { get; set; }

            public _身份信息() 
            {
                专家证电子扫描件=new List<string>();
            }
        }
        public List<供应商._产品类别> 可参评物资类别列表 { get; set; }
        public _学历信息 学历信息 { get; set; }
        public class _学历信息
        {
            public string 毕业院校 { get; set; }
            [Required(ErrorMessage = "请填写专业技术职称！")]
            public 专业技术职称 专业技术职称 { get; set; }
            [Required(ErrorMessage = "请填写时间！")]
            [DataType(DataType.Date)]
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 取得现技术职称时间 { get; set; }
            [Required(ErrorMessage = "请上传扫描件！")]
            public List<string> 职称证书电子扫描件 { get; set; }
            public 学历 最高学历 { get; set; }
            public 学位 最高学位 { get; set; }
            public string 最高学位证书 { get; set; }
            public _学历信息()
            {
                职称证书电子扫描件=new List<string>();
            }
        }
        public _工作经历信息 工作经历信息 { get; set; }
        public class _工作经历信息
        {
            [Required(ErrorMessage = "请填写参加工作时间！")]
            [DataType(DataType.Date)]
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 参加工作时间 { get; set; }
            [Required(ErrorMessage = "请填写从事专业！")]
            public string 从事专业 { get; set; }
            [Required(ErrorMessage = "请填写从事专业起始年度！")]
            public int 从事专业起始年度 { get; set; }
            public string 工作单位 { get; set; }
            public _地址信息 单位地址 { get; set; }
            public string 邮编 { get; set; }
            public string 现任职务 { get; set; }
            public 健康状况 健康状况 { get; set; }
            public string 主要工作经历 { get; set; }
            public string 社会兼聘职情况 { get; set; }
            public string 本人参加过何种项目招标及评标 { get; set; }
            public string 本人专业领域研究及成果 { get; set; }
            public string 退休证书 { get; set; }
            public _工作经历信息() { this.单位地址 = new _地址信息(); }
        }
        public bool 黑名单 { get; set; }
        public 专家()
        {
            this.审核数据 = new _审核数据();
            this.历史抽取信息 = new Dictionary<string, List<DateTime>>();
            //this.上次出席评标时间 = DateTime.MinValue;
            this.身份信息 = new _身份信息();
            this.学历信息 = new _学历信息();
            this.工作经历信息 = new _工作经历信息();
            this.可参评物资类别列表 = new List<供应商._产品类别>();
        }
    }
    public enum 采购管理单位
    {
        未选择,
        成都军区物资采购机构_成都,
        成都军区物资采购机构_昆明,
        成都军区物资采购机构_贵阳,
        成都军区物资采购机构_重庆,
    }
    public enum 专家类型
    {
        未填写 = 0,
        技术 = 1,
        法律 = 2,
        经济 = 3,
    }
    public enum 政治面貌
    {
        未填写 = 0,
        党员 = 1,
        团员 = 2,
        群众 = 3,
        民主党派 = 4,
        其他 = 99,
    }
    public enum 专家类别
    {
        未填写 = 0,
        军内 = 1,
        地方 = 2,
    }
    public enum 专家级别
    {
        未填写 = 0,
        全军库专家 = 1,
        军区库专家 = 2,
        地方库专家 = 3,
        注册专家 = 4,
    }
    public enum 证件类型
    {
        未填写 = 0,
        身份证 = 1,
        军官证 = 2,
        //文职干部证 = 3,
        //其他 = 99,
    }
    public enum 民族
    {
        未填写 = 0,
        汉族 = 1,
        壮族 = 2,
        满族 = 3,
        回族 = 4,
        苗族 = 5,
        维吾尔族 = 6,
        土家族 = 7,
        彝族 = 8,
        蒙古族 = 9,
        藏族 = 10,
        布依族 = 11,
        侗族 = 12,
        瑶族 = 13,
        朝鲜族 = 14,
        其他民族 = 99,
    }
    public enum 专业技术职称
    {
        未填写 = 0,
        编辑 = 1,
        编审 = 2,
        播音指导 = 3,
        畜牧师 = 4,
        船长 = 5,
        大副 = 6,
        大管轮 = 7,
        电影放映技师 = 8,
        电影放映主任技师 = 9,
        二等引航员 = 10,
        二级编剧 = 11,
        二级导演 = 12,
        二级飞行机械员 = 13,
        二级飞行通信员 = 14,
        二级飞行员 = 15,
        二级公证员 = 16,
        二级领航员 = 17,
        二级律师 = 18,
        二级美术师 = 19,
        二级文学创作 = 20,
        二级舞美设计师 = 21,
        二级演员 = 22,
        二级演奏员 = 23,
        二级指挥 = 24,
        二级作曲 = 25,
        翻译 = 26,
        副编审 = 27,
        副教授 = 28,
        副研究馆员 = 29,
        副研究员 = 30,
        副译审 = 31,
        副主任护师 = 32,
        副主任技师 = 33,
        副主任药师 = 34,
        副主任医师 = 35,
        高级报务员 = 36,
        高级编辑 = 37,
        高级畜牧师 = 38,
        高级船长 = 39,
        高级电机员 = 40,
        高级工程师 = 41,
        高级工艺美术师 = 42,
        高级关务监督 = 43,
        高级会计师 = 44,
        高级记者 = 45,
        高级讲师 = 46,
        高级教练 = 47,
        高级经济师 = 48,
        高级轮船长 = 49,
        高级农艺师 = 50,
        高级审计师 = 51,
        高级实习指导教师 = 52,
        高级实验师 = 53,
        高级兽医师 = 54,
        高级统计师 = 55,
        高级引航员 = 56,
        高级政工师 = 57,
        工程技术应用研究员 = 58,
        工程师 = 59,
        工艺美术师 = 60,
        关务监督 = 61,
        馆员 = 62,
        国家级教练 = 63,
        会计师 = 64,
        记者 = 65,
        技术编辑 = 66,
        讲师 = 67,
        教授 = 68,
        教授级高级畜牧师 = 69,
        教授级高级农艺师 = 70,
        教授级高级实验师 = 71,
        教授级高级兽医师 = 72,
        经济师 = 73,
        轮机长 = 74,
        农艺师 = 75,
        三级编剧 = 76,
        三级导演 = 77,
        三级公证员 = 78,
        三级律师 = 79,
        三级美术师 = 80,
        三级文学创作 = 81,
        三级舞美设计师 = 82,
        三级演员 = 83,
        三级演奏员 = 84,
        三级指挥 = 85,
        三级作曲 = 86,
        审计师 = 87,
        实验师 = 88,
        兽医师 = 89,
        通用报务员 = 90,
        通用电机员 = 91,
        统计师 = 92,
        舞台技师 = 93,
        小学高级教师 = 94,
        研究馆员 = 95,
        研究员 = 96,
        一等报务员 = 97,
        一等电机员 = 98,
        一等引航员 = 99,
        一级编剧 = 100,
        一级播音员 = 101,
        一级导演 = 102,
        一级飞行机械员 = 103,
        一级飞行通信员 = 104,
        一级飞行员 = 105,
        一级公证员 = 106,
        一级教练 = 107,
        一级领航员 = 108,
        一级律师 = 109,
        一级美术师 = 110,
        一级实习指导教师 = 111,
        一级文学创作 = 112,
        一级舞美设计师 = 113,
        一级校对 = 114,
        一级演员 = 115,
        一级演奏员 = 116,
        一级指挥 = 117,
        一级作曲 = 118,
        译审 = 119,
        幼儿园高级教师 = 120,
        政工师 = 121,
        中学高级教师 = 122,
        中学一级教师 = 123,
        主管护师 = 124,
        主管技师 = 125,
        主管药师 = 126,
        主任编辑 = 127,
        主任播音员 = 128,
        主任护师 = 129,
        主任记者 = 130,
        主任技师 = 131,
        主任舞 = 132,
        主任药师 = 133,
        主任医师 = 134,
        主治主管医师 = 135,
        助理研究员 = 136,
        其他 = 9999,
    }
    public enum 学历
    {
        未填写 = 0,
        大专及以下 = 1,
        本科 = 2,
        研究生 = 3,
        博士 = 4,
    }
    public enum 学位
    {
        未填写 = 0,
        无 = 1,
        学士 = 2,
        硕士 = 3,
        博士 = 4,
        博士后 = 5,
    }
    public enum 健康状况
    {
        未填写 = 0,
        良好 = 1,
        一般 = 2,
        其他 = 99,
    }

    public class 专家可评标专业
    {
        public static List<string> 非商品分类评审专业
            = Mongo.Coll<专家可评标专业>().FindAllAs<BsonDocument>().Select(o=>o["专业名"].AsString).ToList();
        public string 专业名 { get; set; }
        public static void 添加可评标专业(string 专业名称)
        {
            var c = Mongo.Coll<专家可评标专业>();
            if (0 == c.Count(Query<专家可评标专业>.EQ(o => o.专业名, 专业名称)))
                c.Insert(new 专家可评标专业 {专业名 = 专业名称});
            if (!非商品分类评审专业.Contains(专业名称)) 非商品分类评审专业.Add(专业名称);
        }
    }


    public class 专家可评标专业分类
    {
        public long Id { get; set; }
        public string 分类名 { get; set; }
        public List<string> 子分类 { get; set; }

        public static List<专家可评标专业分类> 评审专业
            = Mongo.Coll<专家可评标专业分类>().FindAllAs<专家可评标专业分类>().ToList();

        public 专家可评标专业分类()
        {
            子分类 = new List<string>();
        }
    }





}

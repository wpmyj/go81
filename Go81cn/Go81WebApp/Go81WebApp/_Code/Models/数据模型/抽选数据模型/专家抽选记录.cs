using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Options;

namespace Go81WebApp.Models.数据模型
{
#if INTRANET
    public class 专家抽选记录 : 基本数据模型
    {
        public string 操作人姓名 { get; set; }
        public string 操作人电话 { get; set; }
        public 用户链接<单位用户> 经办人 { get; set; }
        public 用户链接<单位用户> 批准人 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 申请时间 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 批准时间 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 操作时间 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 确认时间 { get; set; }
        public 申请抽选状态 申请抽选状态 { get; set; }
        public string 项目名称 { get; set; }
        public string 项目编号 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 评标时间 { get; set; }
        public List<用户链接<专家>> 回避专家列表 { get; set; }
        public List<_专家抽选条件> 专家抽选条件 { get; set; }
        public bool 是否已评分 { get; set; }
        public bool 是否一体机抽取 { get; set; }
        public class _专家抽选条件
        {
            public string 描述 { get; set; }
            public int 需要专家数量 { get; set; }
            public List<专家类别> 专家类别 { get; set; }
            public string 模糊查询 { get; set; }
            public _地域 所在地区 { get; set; }
            public List<_评标产品类别> 可评标产品类别 { get; set; }
            public class _评标产品类别 : 供应商._产品类别
            {
                public bool 二级分类可用专家不足返回一级分类 { get; set; }
            }
           
            //高级选项
            public List<专家类型> 专家类型 { get; set; }
            public List<专家级别> 专家级别 { get; set; }
            public List<专业技术职称> 专业技术职称 { get; set; }
            public List<学历> 学历要求 { get; set; }
            public List<学位> 学位要求 { get; set; }
            public _专家抽选条件()
            {
                this.所在地区 = new _地域();
                this.可评标产品类别 = new List<_评标产品类别>();
                this.专家类别 = new List<专家类别>();
                this.专家级别 = new List<专家级别>();
                this.专家类型 = new List<专家类型>();
                this.专业技术职称 = new List<专业技术职称>();
                this.学历要求 = new List<学历>();
                this.学位要求 = new List<学位>();
            }
        }
        public int 总计预定抽选专家数 { get; set; }
        public List<_专家抽选条目> 抽选专家列表 { get; set; }
        public bool 是否已打印 { get; set; }
        public class _专家抽选条目
        {
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 抽选时间 { get; set; }
            public bool 预定出席 { get; set; }
            public string 不能出席原因 { get; set; }
            public 专家评分 专家评分 { get; set; }
            public 用户链接<专家> 专家 { get; set; }
            public _专家抽选条目() { this.专家 = new 用户链接<专家>();this.专家评分 = new 专家评分(); }
        }

        public class 专家评分
        {
            public 专家水平评分 专家水平评分 { get; set; }
            public 专家评标态度评分 专家评标态度评分 { get; set; }
        }
        public 专家抽选记录()
        {
            this.经办人 = new 用户链接<单位用户>();
            this.批准人 = new 用户链接<单位用户>();
            this.专家抽选条件 = new List<_专家抽选条件>();
            this.抽选专家列表 = new List<_专家抽选条目>();
            this.回避专家列表 = new List<用户链接<专家>>();
        }
    }
    public enum 申请抽选状态
    {
        未提交 = 0,
        已提交待批准 = 1,
        已批准待抽选 = 2,
        已完成抽选 = 3,
        未获批准 = 4,
    }
    public enum 专家水平评分
    {
        未设置 = 0,
        优 = 5,
        良 =3,
        中 = 1,
        差 = -2
    }
    public enum 专家评标态度评分
    {
        未设置 = 0,
        非常好 = 5,
        好 = 3,
        一般 = 1,
        不好 = -1,
        非常不好 = -5
    }
#endif
}
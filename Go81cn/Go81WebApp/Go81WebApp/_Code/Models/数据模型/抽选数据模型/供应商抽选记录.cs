using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Go81WebApp.Models.数据模型
{
#if INTRANET
    public class 供应商抽选记录 : 基本数据模型
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
        public List<_供应商抽选条件> 供应商抽选条件 { get; set; }
        public class _供应商抽选条件
        {
            public string 描述 { get; set; }
            public int 需要供应商数量 { get; set; }
            public List<_地域> 所在地区 { get; set; }
            public List<_提供产品类别> 可提供产品类别 { get; set; }
            public class _提供产品类别 : 供应商._产品类别
            {
                public bool 二级分类可用供应商不足返回一级分类 { get; set; }
            }
            //高级选项
            public _供应商抽选条件()
            {
                所在地区 = new List<_地域>();
                可提供产品类别 = new List<_提供产品类别>();

            }
        }
        public List<用户链接<供应商>> 回避供应商列表 { get; set; }
        public int 总计预定抽选供应商数 { get; set; }
        public List<_供应商抽选条目> 抽选供应商列表 { get; set; }
        public class _供应商抽选条目
        {
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 抽选时间 { get; set; }
            public bool 预定出席 { get; set; }
            public string 不能出席原因 { get; set; }
            public 用户链接<供应商> 供应商 { get; set; }
            public _供应商抽选条目() { this.供应商 = new 用户链接<供应商>(); }
        }
        public 供应商抽选记录()
        {
            this.经办人 = new 用户链接<单位用户>();
            this.批准人 = new 用户链接<单位用户>();
            this.供应商抽选条件 = new List<_供应商抽选条件>();
            this.抽选供应商列表 = new List<_供应商抽选条目>();
            this.回避供应商列表 = new List<用户链接<供应商>>();
        }
    }
#endif
}

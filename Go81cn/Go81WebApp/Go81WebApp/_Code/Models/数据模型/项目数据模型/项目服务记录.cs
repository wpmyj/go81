using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.管理器;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型.项目数据模型
{
    public class 项目服务记录 : 基本数据模型
    {
        public 用户链接<供应商> 供应商链接 { get; set; }
        public 用户链接<单位用户> 验收单位链接 { get; set; }
        public 招标采购项目链接 所属项目 { get; set; }
        public _服务信息 服务信息 { get; set; }
        public class _服务信息
        {
            public string 验收单电子扫描件 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 验收时间 { get; set; }
            [Required(ErrorMessage="验收单位编码是必须的")]
            public string 验收单位编码 { get; set; }
            public string 事务概要 { get; set; }
        }
        public _服务评价 服务评价 { get; set; }
        public class _服务评价
        {
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 评价时间 { get; set; }
            public string 服务评价内容 { get; set; }
            public 服务评级 服务评级 { get; set; }
        }
        public enum 服务评级
        {
            未填写 = 0,
            好评 = 5,
            中评 = 1,
            差评 = -5,
        }
        public 项目服务记录()
        {
            供应商链接 = new 用户链接<供应商>();
            验收单位链接 = new 用户链接<单位用户>();
            所属项目 = new 招标采购项目链接();
            服务信息 = new _服务信息();
            服务评价 = new _服务评价();
        }
    }
    public class 项目服务记录链接
    {
        public long 项目服务记录ID { get; set; }
        public 项目服务记录 项目服务记录 { get { return 项目服务记录管理.查找项目服务记录(项目服务记录ID); } }
        public 项目服务记录链接() { this.项目服务记录ID = -1; }
    }
}
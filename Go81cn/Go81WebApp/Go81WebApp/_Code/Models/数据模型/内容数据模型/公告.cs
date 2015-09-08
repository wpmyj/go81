using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.推送数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Ajax.Utilities;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型.内容数据模型
{
    public class 公告 : 内容基本数据
    {
        public _内容来源 公告来源 { get; set; }
        public _公告信息 公告信息 { get; set; }
        public class _公告信息
        {
            public _地域 所属地域 { get; set; }
            public string 一级分类 { get; set; }
            public string 二级分类 { get; set; }
            public string 三级分类 { get; set; }
            public string 公告编号 { get; set; }
            public 公告版本 公告版本 { get; set; }
            public 公告类别 公告类别 { get; set; }
            public 公告性质 公告性质 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 过期时间 { get; set; }
            public string 需求单位 { get; set; }
            public bool 是否撤回 { get; set; }
            public 公告推送链接 公告推送 { get; set; }
            public bool 已推送 { get { return null != 公告推送 && -1 != this.公告推送.公告推送ID; } }
            public _公告信息() { this.所属地域 = new _地域(); }
        }
        public _项目信息 项目信息 { get; set; }
        public class _项目信息
        {
            [Required(ErrorMessage = "项目名称必须填写")]
            public string 项目名称 { get; set; }
            //[Required(ErrorMessage = "项目编号必须填写")]
            public string 项目编号 { get; set; }
            public decimal 预算金额 { get; set; }
            public List<string> 项目标的物 { get; set; }
        }
        public _审核数据 审核数据2 { get; set; }
        public List<商品链接> 中标商品链接 { get; set; } 
        public enum 公告版本
        {
            正常 = 0,
            变更 = 1,
            更正 = 2,
            补遗 = 3,
        }
        public enum 公告类别
        {
            未设置 = 0,
            公开招标 = 1,
            邀请招标 = 2,
            协议采购 = 3,
            单一来源 = 4,
            询价采购 = 5,
            竞争性谈判 = 6,
            网上竞标 = 11,
            其他 = 9999,
        }
        public enum 公告性质
        {
            未设置 = 0,
            预公告 = 1,
            技术公告 = 2,
            采购公告 = 3,
            中标结果公示 = 4,
            废标公告 = 5,
            流标公告 = 6,
            需求公告=7,
            答疑公告=8,
            变更公告=9,
            更正公告=10,
            补遗公告=11,
        }
        public 公告()
        {
            this.公告来源 = new _内容来源();
            this.公告信息 = new _公告信息();
            this.项目信息 = new _项目信息();
            this.审核数据2 = new _审核数据();
            this.中标商品链接 = new List<商品链接>();
        }
    }
    public class 公告链接
    {
        public long 公告ID { get; set; }
        public 公告 公告 { get { return 公告管理.查找公告(公告ID); } }
        public 公告链接() { this.公告ID = -1; }
    }
}
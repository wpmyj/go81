using Go81WebApp.Models.数据模型.用户数据模型;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using Helpers;

namespace Go81WebApp.Models.数据模型.内容数据模型
{
    public abstract class 内容基本数据 : 基本数据模型
    {
        public _内容基本信息 内容基本信息 { get; set; }
        public _内容主体 内容主体 { get; set; }
        public class _内容基本信息
        {
            public 重要程度 重要程度 { get; set; }
            public bool 允许评论 { get; set; }
            public 用户链接<用户基本数据> 所有者 { get; set; }
            public _内容基本信息() { this.重要程度 = 0; this.允许评论 = false; this.所有者 = new 用户链接<用户基本数据>(); }
        }
        public class _内容主体
        {
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 发布时间 { get; set; }
            [Required(ErrorMessage = "标题必须填写")]
            public string 标题 { get; set; }
            public string 摘要 { get; set; }
            public string 内容 { get; set; }
            public List<string> 图片 { get; set; }
            public List<string> 音频 { get; set; }
            public List<string> 视频 { get; set; }
            public List<string> 附件 { get; set; }
        }
        public _审核数据 审核数据 { get; set; }
        public long 点击次数 { get; set; }
        public 内容基本数据(bool 允许评论 = false) //: base()
        {
            this.内容基本信息 = new _内容基本信息() { 允许评论 = 允许评论 };
            this.内容主体 = new _内容主体();
            this.审核数据 = new _审核数据();
            this.点击次数 = RandomString.ClickStartNum();
        }
    }
    public class _审核数据
    {
        public 审核状态 审核状态 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 审核时间 { get; set; }
        public 用户链接<用户基本数据> 审核者 { get; set; }
        public string 未通过理由 { get; set; }
        public _审核数据() { this.审核者 = new 用户链接<用户基本数据>(); }
    }
}
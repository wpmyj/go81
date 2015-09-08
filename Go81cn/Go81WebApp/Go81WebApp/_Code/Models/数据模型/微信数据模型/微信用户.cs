using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Go81WebApp.Models.数据模型.微信数据模型
{
    public class 微信用户:基本数据模型
    {
        public 用户链接<用户基本数据> 用户链接 { get; set; }
        public string openId { get; set; }
        public bool 已验证 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 验证时间 { get; set; }
        public 微信用户()
        {
            this.用户链接 = new 用户链接<用户基本数据>();
        }
    }
}
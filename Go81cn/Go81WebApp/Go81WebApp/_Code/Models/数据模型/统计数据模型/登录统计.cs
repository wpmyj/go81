using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Go81WebApp.Models.数据模型.统计数据模型
{
    public class 登录统计 : 基本数据模型
    {
        public 用户链接<用户基本数据> 用户数据 { get; set; }
        public string 登录IP { get; set; }
        public 登录结果 登录结果 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 登录时间 { get { return 基本数据.添加时间; } }
        public bool 内网访问 { get; set; }
        public 登录统计()
        {
            this.用户数据 = new 用户链接<用户基本数据>();
        }
    }
    public enum 登录结果
    {
        未设置 = 0,
        登录成功 = 1,
        用户名或密码错误 = 2,
        登录失败 = 3,
    }
}
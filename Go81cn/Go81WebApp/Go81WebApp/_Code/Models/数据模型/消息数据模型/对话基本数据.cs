using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;

namespace Go81WebApp.Models.数据模型.消息数据模型
{
    public abstract class 对话基本数据 : 基本数据模型
    {
        public 对话参与者链接 发起者 { get; set; }
        public IEnumerable<对话消息> 对话消息 { get { return 对话消息管理.查询对话消息(0, 0, Query.And(Query.EQ("所属对话.对话ID", Id), Query.EQ("所属对话.对话类型", this.GetType().Name)), false, SortBy.Ascending("基本数据.添加时间")); } }
        protected 对话基本数据() { this.发起者 = new 对话参与者链接(); }
    }
    public class 对话链接
    {
        public long 对话ID { get; set; }
        public string 对话类型 { get; private set; }
        public 对话基本数据 对话 { get { if (null == 对话类型) return null; var c = Mongo.Coll(对话类型); return null == c ? null : c.FindOneByIdAs(Type.GetType("Go81WebApp.Models.数据模型.消息数据模型." + 对话类型), 对话ID) as 对话基本数据; } }
        public 对话链接() { this.对话ID = -1; }
        public string 设置对话类型(Type t) { 对话类型 = t.IsSubclassOf(typeof(对话基本数据)) ? t.Name : null; return 对话类型; }
    }
    public class 对话参与者链接 : 用户链接<用户基本数据>
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 上次阅读时间 { get; set; }
    }
    public class 对话消息 : 基本数据模型
    {
        public 对话链接 所属对话 { get; set; }
        public 用户链接<用户基本数据> 发言人 { get; set; }
        public int 序号 { get; set; }
        public _消息主体 消息主体 { get; set; }
        public class _消息主体
        {
            public string 标题 { get; set; }
            public string 内容 { get; set; }
            public List<string> 图片 { get; set; }
            public List<string> 附件 { get; set; }
        }
        public 对话消息()
        {
            this.所属对话 = new 对话链接();
            this.发言人 = new 用户链接<用户基本数据>();
            this.消息主体 = new _消息主体();
        }
    }
    public enum 处理状态
    {
        未处理 = 0,
        已处理 = 1,
        处理中 = 2,
        已转发 = 3,
        全部 = 99,
    }
}
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型
{
    public abstract class 基本数据模型
    {
        public long Id { get; set; }
        public string HashCode { get { return Hash.Compute(this.ToJson()); } }
        public _基本数据 基本数据 { get; set; }
        public class _基本数据
        {
            public bool 已删除 { get; set; }
            public bool 已屏蔽 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 添加时间 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 修改时间 { get; set; }
            public List<string> 标签 { get; set; }
            public string 备注 { get; set; }
            public _基本数据()
            {
                已删除 = false;
                已屏蔽 = false;
                添加时间 = DateTime.Now;
                修改时间 = DateTime.Now;
                标签 = new List<string>();
            }
        }

        public 基本数据模型()
        {
            this.Id = -1;
            this.基本数据 = new _基本数据();
        }
    }
    public enum 审核状态
    {
        未审核 = 0,
        审核通过 = 1,
        审核未通过 = 2,
    }
    public enum 重要程度
    {
        未指定 = 0,
        一般 = 100,
        重要 = 200,
        特别重要 = 300,
        必读 = 400,
    }
    public class _地域
    {
        public string 省份 { get; set; }
        public string 城市 { get; set; }
        public string 区县 { get; set; }
        public string 地域 { get { return 省份 + 城市 + 区县; } }
    }
    public class 数据链接
    {
        public string 完整数据类型 { get; private set; }
        public string 数据类型 { get { return null == 完整数据类型 ? null : 完整数据类型.Substring(完整数据类型.LastIndexOf('.') + 1); } }
        public long 数据ID { get; set; }
        public 基本数据模型 数据
        {
            get
            {
                if (null == 完整数据类型) return null;
                var c = Mongo.Coll(数据类型);
                return null == c
                    ? null
                    : c.FindOneByIdAs(Type.GetType(完整数据类型), 数据ID) as 基本数据模型
                    ;
            }
        }
        public 数据链接() { this.数据ID = -1; }
        public void 设置数据类型(Type t) { if (t.IsSubclassOf(typeof(基本数据模型))) this.完整数据类型 = t.FullName; }
    }
    public class 操作数据
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 操作时间 { get; set; }
        public string 操作描述 { get; set; }
        public 用户链接<用户基本数据> 操作人员 { get; set; }

        public 操作数据() { this.操作人员 = new 用户链接<用户基本数据>(); }
    }
}

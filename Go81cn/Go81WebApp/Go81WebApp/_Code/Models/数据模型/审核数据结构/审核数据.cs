using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Go81WebApp.Models.数据模型.审核数据结构
{
    public class 审核规则<T>
    {
        public void Process(T o)
        {
            //if()
        }
    }

    public class 审核数据<T> : 基本数据模型
    {
        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, 审核数据项> 审核明细 { get; set; }

        public 审核数据()
        {
            审核明细 = new Dictionary<string, 审核数据项>();
        }
    }

    public class 审核数据项
    {
        public 审核状态 审核状态 { get; set; }
        public string 审核不通过理由 { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 审核时间 { get; set; }

        public 用户链接<用户基本数据> 审核者 { get; set; }
        public string 审核人 { get; set; }
        public string 审核人电话 { get; set; }

        public 审核数据项()
        {
            审核者 = new 用户链接<用户基本数据>();
        }
    }
}
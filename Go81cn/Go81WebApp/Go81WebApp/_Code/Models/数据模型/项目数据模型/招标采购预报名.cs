using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.管理器;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Go81WebApp.Models.数据模型.项目数据模型
{
    public class 招标采购预报名 : 基本数据模型
    {
        public 公告链接 所属公告链接 { get; set; }
        public bool 预报名已关闭 { get; set; }
        public List<_供应商预报名信息> 预报名供应商列表 { get; set; }
        public List<_供应商所需资料> 供应商所需资料 { get; set; }
        public class _供应商所需资料
        {
            public string 资料名;
            public bool 图片;
        }

        public class _供应商预报名信息
        {
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 报名时间 { get; set; }
            public 用户链接<供应商> 供应商链接 { get; set; }
            public bool 已购买标书 { get; set; }
            public bool 已发送电子标书 { get; set; }
            [BsonDictionaryOptions(DictionaryRepresentation.Document)]
            public Dictionary<string, List<string>> 供应商提交资料 { get; set; }
            public 内容数据模型._审核数据 审核数据 { get; set; }
            public _供应商预报名信息()
            {
                this.供应商链接 = new 用户链接<供应商>();
                this.供应商提交资料 = new Dictionary<string, List<string>>();
                this.审核数据 = new 内容数据模型._审核数据();
            }
        }
        public _标书信息 标书信息 { get; set; }
        public class _标书信息
        {
            public string 备注 { get; set; }
            public List<string> 内容 { get; set; }
            public _标书信息()
            {
                this.内容 = new List<string>();
            }
        }
        public 招标采购预报名()
        {
            所属公告链接 = new 公告链接();
            预报名供应商列表 = new List<_供应商预报名信息>();
            供应商所需资料 = new List<_供应商所需资料>();
            this.标书信息 = new _标书信息();
        }
    }

    public class 招标采购预报名链接
    {
        public long 招标采购预报名ID { get; set; }
        public 招标采购预报名 招标采购预报名 { get { return 招标采购预报名管理.查找招标采购预报名(招标采购预报名ID); } }
        public 招标采购预报名链接() { this.招标采购预报名ID = -1; }
    }
}
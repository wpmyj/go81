using System;
using System.Collections.Generic;
using System.Linq;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.管理器;
using System.ComponentModel.DataAnnotations;
using MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Go81WebApp.Models.数据模型.项目数据模型
{
    public class 机票验收单 : 基本数据模型
    {
        public string 验收单号 { get; set; }
        public 用户链接<供应商> 供应商链接 { get; set; }
        public string 供货单位承办人 { get; set; }
        public string 供货单位承办人电话 { get; set; }
        public List<_机票服务条目> 服务列表 { get; set; }
        public class _机票服务条目
        {
            public string 航班号 { get; set; }
            public string 客户姓名 { get; set; }
            public string 行程单号 { get; set; }
            public string 验证附件路径 { get; set; }
            public string 计量单位 { get; set; }
            public int 数量 { get; set; }
            public decimal 单价 { get; set; }
            public 机票验证状态 验证状态 { get; set; }
        }
     
        public bool 是否已经打印 { get; set; }
        public List<_打印信息> 打印信息 { get; set; }
        public bool 是否作废 { get; set; }

        public 机票验收单()
        {
            var d = DateTime.Now;
            验收单号 = string.Format("{0:D4}{1:D3}{2:D6}", d.Year, d.DayOfYear, Mongo.NextId<验收单>(d.Year.ToString("D:4")));
            Id = long.Parse(验收单号);
            供应商链接 = new 用户链接<供应商>();
            服务列表 = new List<_机票服务条目>();
            打印信息 = new List<_打印信息>();
        }
    }
    public class 机票验收单链接
    {
        public long 机票验收单ID { get; set; }
        public 机票验收单 机票验收单 { get { return 机票验收单管理.查找机票验收单(机票验收单ID); } }
        public 机票验收单链接() { this.机票验收单ID = -1; }
    }
    public enum 机票验证状态
    {
        待验证=0,
        已验证=1,
        查无此票 = 2,
        价格不匹配=3,
        行程不匹配=4,
    }
}
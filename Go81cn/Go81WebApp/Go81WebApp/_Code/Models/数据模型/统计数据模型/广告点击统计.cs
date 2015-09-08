using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Go81WebApp.Models.数据模型.统计数据模型
{
    public class 广告点击统计:基本数据模型
    {
        public string 广告位置 { get; set; }
        public 用户链接<供应商> 广告所属供应商 { get; set; }
        public bool 游客点击 { get; set; }
        public 用户链接<用户基本数据> 点击用户 { get; set; }
        public 广告类型 广告类型 { get; set; }
        public 商品链接 商品链接 { get; set; }
        public string 点击IP { get; set; }
        public DateTime 点击时间 { get { return 基本数据.添加时间; } }
        public bool 内网访问 { get; set; }
        public 广告点击统计()
        {
            this.广告所属供应商 = new 用户链接<供应商>();
            this.点击用户 = new 用户链接<用户基本数据>();
            this.商品链接 = new 商品链接();
        }
        
    }
    public enum 广告类型
    {
        未设置 = 0,
        商品广告 = 1,
        供应商广告 = 2,
    }
}
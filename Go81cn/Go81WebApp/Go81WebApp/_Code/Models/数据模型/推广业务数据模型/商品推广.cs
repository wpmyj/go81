using System;
using Go81WebApp.Models.数据模型.商品数据模型;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型.推广业务数据模型
{
    public class 商品推广 : 基本数据模型
    {
        public enum _广告位置
        {
            热销商品,
            优质商品
        }
        public enum _显示级别
        {
            低价位,
            中等价位,
            高等价位
        }
        public _广告位置 广告位置 { get; set; }
        public 商品链接 商品 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 显示时间 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 结束时间 { get; set; }
        public _显示级别 显示级别 { get; set; }
        public 商品推广()
        {
            this.商品 = new 商品链接();
        }
    }
}
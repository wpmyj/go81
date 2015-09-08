using System;
using Go81WebApp.Models.数据模型.用户数据模型;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
namespace Go81WebApp.Models.数据模型.推广业务数据模型
{
    public class 供应商推广 : 基本数据模型
    {
        public enum _广告位置
        {
            新入库,
            诚信供应商
        }
        public enum _显示级别
        {
            军采通会员,
            军采通标准会员,
            军采通超级会员
        }
        public _广告位置 广告位置 { get; set; }
        public 用户链接<供应商> 供应商 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 显示时间 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 结束时间 { get; set; }
        public _显示级别 显示级别 { get; set; }
        public 供应商推广()
        {
            this.供应商 = new 用户链接<供应商>();
        }
    }
}
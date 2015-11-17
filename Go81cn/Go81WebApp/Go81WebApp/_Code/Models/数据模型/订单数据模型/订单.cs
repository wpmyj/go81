using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp._Code.Models.管理器.订单管理;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型.订单数据模型
{
    public class 订单 : 基本数据模型
    {
        public 用户链接<个人用户> 订单所属用户 { get; set; }
        public List<商品订单> 商品订单列表 { get; set; }
        public bool 使用优惠码 { get; set; }
        public bool 已付款 { get; set; }
        public 优惠券链接 使用优惠券 { get; set; }
        public _地域 收货地址 { get; set; }
        public string 联系电话 { get; set; }
        public string 联系人 { get; set; }
        public string 详细地址 { get; set; }
        public decimal 订单商品总价格 { get; set; }
        public decimal 总运费 { get; set; }

        public decimal 订单总价格{ get; set; }

        public decimal 订单总付款 { get; set; }

        public 订单()
        {
            this.商品订单列表 = new List<商品订单>();
            this.订单所属用户 = new 用户链接<个人用户>();
            this.使用优惠券 = new 优惠券链接();
            this.收货地址 = new _地域();
        }
    }

    public class 商品订单
    {
        public 商品链接 商品 { get; set; }

        public int 数量 { get; set; }
        public bool 已发货 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 发货时间 { get; set; }
        public string 运单号 { get; set; }
        public 商品订单()
        {
            this.商品 = new 商品链接();
        }
    }
    public class 优惠券信息 : 基本数据模型
    {
        public 优惠券 优惠券 { get; set; }
        public 用户链接<个人用户> 优惠券所属用户 { get; set; }
        public bool 已使用 { get; set; }

        public 优惠券信息()
        {
            this.优惠券所属用户 = new 用户链接<个人用户>();
        }
    }
    public enum 优惠券
    {
        无优惠 = 0,
        满200减10 = 10,
        满300减20 = 20,
        满500减50 = 50,
    }
    public class 优惠券链接
    {
        public long 优惠券ID { get; set; }
        public 优惠券信息 优惠券信息 { get { return 优惠券信息管理.查找优惠券信息(优惠券ID); } }
        public 优惠券链接() { 优惠券ID = -1; }
    }
}
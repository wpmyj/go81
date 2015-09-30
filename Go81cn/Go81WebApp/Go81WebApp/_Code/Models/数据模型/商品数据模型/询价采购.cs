using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;

namespace Go81WebApp._Code.Models.数据模型.商品数据模型
{
    public class 询价采购 : 基本数据模型
    {
        public 商品链接 询价商品 { get; set; }
        public List<_议价列表> 议价列表 { get; set; }
        public _附加信息 附加信息 { get; set; }
        public string 订单号 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 生成日期 { get; set; }
        public bool 订单状态 { get; set; }
        public 询价采购()
        {
            this.询价商品 = new 商品链接();
            this.议价列表 = new List<_议价列表>();
            this.附加信息 = new _附加信息();
        }
    }
    public class 商品链接
    {
        public long 商品ID { get; set; }
        public 商品 商品 { get { return 商品管理.查找商品(商品ID); } }
        public 商品链接() { this.商品ID = -1; }
    }
    public class _议价列表
    {
        public 用户链接<供应商> 供应商 { get; set; }

        public 商品链接 议价商品 { get; set; }
        public decimal 价格
        {
            get
            {
                var p = 商品管理.查询供应商商品(供应商.用户ID, 0, 0, Query<商品>.Where(o => o.商品信息.精确型号 == 议价商品.商品.商品信息.精确型号));
                return p.Count() > 0 ? p.First().销售信息.价格 : -1;
            }
        }
        public decimal 议价 { get; set; }
        public decimal 回复价格 { get;set;}
        public int 数量 { get; set; }
        public string 备注 { get; set; }
        public decimal 集成费 { get; set; }
        public decimal 管线费 { get; set; }
        public decimal 服务费 { get; set; }
        public decimal 原价合计 { get { return 数量 * 价格 + 集成费 + 管线费 + 服务费; } }
        public decimal 现价合计 { get { return 数量 * 回复价格 + 集成费 + 管线费 + 服务费; } }
        public bool 交易状态 { get; set; }
        public _议价列表()
        {
            this.供应商 = new 用户链接<供应商>();
            this.议价商品 = new 商品链接();
        }
    }
     public class _附加信息
     {
         public 商品链接 商品 { get; set; }
         public _预算金额 预算金额 { get; set; }
         public _预算金额 实际金额 { get; set; }
         public _支付方式 支付方式 { get; set; }
         public string 所属行业 { get; set; }
         public string 收货地址 { get; set; }
         public _附加信息()
         {
             this.商品 = new 商品链接();
         }
     }
     public enum _支付方式
     {
         后台直充 = 0,
         支付宝 = 1,
         银行转账 = 2,
     }
     public class _预算金额 
     {
         public decimal 预算内 { get; set; }
         public decimal 预算外 { get; set; }
         public decimal 自筹 { get; set; }
         public decimal 合计 { get { return 预算内 + 预算外 + 自筹; } }
     }
}
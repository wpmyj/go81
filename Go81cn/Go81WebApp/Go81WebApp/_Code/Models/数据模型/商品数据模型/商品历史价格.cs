using System;
using Go81WebApp.Models.数据模型.商品数据模型;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型.商品数据模型
{
    public class 商品历史销售信息 : 基本数据模型
    {
        public 商品链接 所属商品 { get; set; }
        //public 商品库存链接 所属商品库存 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 价格修改日期 { get; set; }
        public decimal 价格 { get; set; }
        public int 销量 { get; set; }
        public long 点击量 { get; set; }
        public 商品._价格属性组合 价格属性组合 { get; set; }
        //public 商品库存._价格属性组合 价格属性组合 { get; set; }
        public 商品历史销售信息()
        {
            this.所属商品 = new 商品链接();
            //this.所属商品库存 = new 商品库存链接();
            this.价格属性组合 = new 商品._价格属性组合();
            //this.价格属性组合 = new 商品库存._价格属性组合();
        }
    }
}
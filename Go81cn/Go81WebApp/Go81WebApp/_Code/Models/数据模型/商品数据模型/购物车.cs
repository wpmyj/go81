using Go81WebApp.Models.数据模型.用户数据模型;
using System.Collections.Generic;

namespace Go81WebApp.Models.数据模型.商品数据模型
{
    public class 购物车 : 基本数据模型
    {
        public 用户链接<用户基本数据> 所属用户 { get; set; }
        public List<选购商品> 选购商品列表 { get; set; }
        public class 选购商品
        {
            public 商品链接 商品 { get; set; }
            public int 数量 { get; set; }
        }
    }
}
using Go81WebApp.Models.数据模型.推广业务数据模型;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Go81WebApp.Models.管理器.推广业务管理
{
    public class 商品推广管理 : 管理器<商品推广>
    {
        public static readonly 商品推广管理 管理器 = new 商品推广管理();
    }
}
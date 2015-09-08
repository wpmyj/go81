using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型.推广业务数据模型;
using Go81WebApp.Models.管理器;

namespace Go81WebApp.Models.管理器.推广业务管理
{
    public class 供应商推广管理 : 管理器<供应商推广>
    {
        public static readonly 供应商推广管理 管理器 = new 供应商推广管理();
    }
}
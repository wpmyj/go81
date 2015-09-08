using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型.用户数据模型;

namespace Go81WebApp.Models.数据模型.内容数据模型
{
    public class 办事指南 : 内容基本数据
    {
        public _内容来源 信息来源 { get; set; }
    }
}
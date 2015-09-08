using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;

namespace Go81WebApp._Code.Models.数据模型.用户数据模型
{
    public class 登录记录 : 基本数据模型
    {
        public 用户链接<用户基本数据> 用户链接 { get; set; }
        public string 登录IP { get; set; }
        public 登录记录()
        {
            用户链接 = new 用户链接<用户基本数据>();
        }
    }
}
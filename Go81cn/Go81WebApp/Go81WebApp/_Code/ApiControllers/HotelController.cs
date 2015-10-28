using System.Web.Mvc;
using Go81WebApp.Models.数据模型.用户数据模型;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Go81WebApp.Models.管理器;

namespace Go81WebApp._Code.ApiControllers
{
    public class HotelController : ApiController
    {
        [System.Web.Mvc.HttpPost]
        public object PostLogin()
        {
            var prs = HttpContext.Current.Request.Params;
            var LoginName = prs["LoginName"];
            var LoginPwd = prs["LoginPwd"];
            if (string.IsNullOrWhiteSpace(LoginName) || string.IsNullOrWhiteSpace(LoginPwd))
            {
                return new 
                {
                    status = -1, message = "用户名或密码不能为空"
                };
            }
            //HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];//获取传统context
            用户基本数据 u;
            if (!用户管理.验证登录名和密码(LoginName, LoginPwd, out u))
            {
                return new
                {
                    status = -1, message = "用户名或密码不能为空" 
                };
            }
            return new
            {
                status = 0, name = u.登录信息.登录名
            };
        }
    }
}

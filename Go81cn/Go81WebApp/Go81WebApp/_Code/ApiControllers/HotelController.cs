using System.IO;
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
        public object Login()
        {
            try
            {
                var prs = HttpContext.Current.Request.Params;
                var LoginName = prs["LoginName"];
                var LoginPwd = prs["LoginPwd"];

                //WriteTxt(LoginName + "           1111111111\r\n");
                //WriteTxt(LoginPwd + "          111111111111\r\n");
                if (string.IsNullOrWhiteSpace(LoginName) || string.IsNullOrWhiteSpace(LoginPwd))
                {
                    return new
                    {
                        status = -1,
                        message = "用户名或密码不能为空"
                    };
                }
                //HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];//获取传统context
                用户基本数据 u;
                if (!用户管理.验证登录名和密码(LoginName, LoginPwd, out u))
                {
                    return new
                    {
                        status = -1,
                        message = "该用户不存在"
                    };
                }
                else
                {
                    //if (typeof(个人用户) != u.GetType())
                    //{
                    //    return new
                    //    {
                    //        status = -1,
                    //        message = "该用户不存在"
                    //    };
                    //}
                    //else
                    //{
                    return new
                    {
                        status = 0,
                        message = u.Id.ToString()
                    };
                    //}
                }
            }
            catch (Exception e)
            {
                //WriteTxt(e.ToString());
                return "";
            }
        }
        [System.Web.Mvc.HttpGet]
        public object GetInfo()
        {
            try
            {
                var prs = HttpContext.Current.Request.Params;
                var LoginName = prs["LoginName"];
                var LoginPwd = prs["LoginPwd"];

                //WriteTxt(LoginName + "           1111111111\r\n");
                //WriteTxt(LoginPwd + "          111111111111\r\n");
                if (string.IsNullOrWhiteSpace(LoginName) || string.IsNullOrWhiteSpace(LoginPwd))
                {
                    return new
                    {
                        status = -1,
                        message = "用户名或密码不能为空"
                    };
                }
                //HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];//获取传统context
                用户基本数据 u;
                if (!用户管理.验证登录名和密码(LoginName, LoginPwd, out u))
                {
                    return new
                    {
                        status = -1,
                        message = "该用户不存在"
                    };
                }
                else
                {
                    //if (typeof(个人用户) != u.GetType())
                    //{
                    //    return new
                    //    {
                    //        status = -1,
                    //        message = "该用户不存在"
                    //    };
                    //}
                    //else
                    //{
                    return new
                    {
                        status = 0,
                        message = u.Id.ToString()
                    };
                    //}
                }
            }
            catch (Exception e)
            {
                //WriteTxt(e.ToString());
                return "error";
            }
        }

        public void WriteTxt(string str)
        {
            StreamWriter sw = File.AppendText("D:\\1.txt");
            sw.Write(str);
            sw.Close();
        }
    }
}

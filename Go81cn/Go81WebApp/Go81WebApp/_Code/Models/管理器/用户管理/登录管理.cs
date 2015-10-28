using Go81WebApp.Models.数据模型.用户数据模型;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Go81WebApp.Models.管理器
{
    public static class 登录管理
    {
        public static Dictionary<long, HttpSessionStateBase> LoginSessions = new Dictionary<long, HttpSessionStateBase>();
        /// <summary>
        /// 未登录返回 -1，否则返回用户ID。
        /// 自动维护 session
        /// </summary>
        /// <param name="hc"></param>
        /// <returns></returns>
        public static long 检查登录(this HttpContextBase hc)
        {
            string temp_session = "";
            if (hc.Session["Ginfo"]!=null)
            {
                temp_session = hc.Session["Ginfo"].ToString();
            }
            long uid = -1;
            if (hc.User.Identity.IsAuthenticated
                && long.TryParse(hc.User.Identity.Name, out uid)
                && 用户管理.检查用户是否存在(uid))
            {
                var u = hc.Session["u"] as 用户基本数据;
                if (null == u || u.Id != uid)
                {
                    u = 用户管理.查找用户(uid);
                    hc.Session.Clear();
                    hc.Session["u"] = u;
                    hc.Session["p"] = 权限管理.计算权限(u);
                    hc.Session["Ginfo"] = temp_session;
                    LoginSessions[uid] = hc.Session;
                }
                return uid;
            }
            登出(hc);
            return -1;
        }
        public static 用户基本数据 登录(this HttpContextBase hc, string loginName, string password, bool noExpire)
        {
            用户基本数据 u;
            if (!用户管理.验证登录名和密码(loginName, password, out u)) return null;
            FormsAuthentication.SetAuthCookie(u.Id.ToString(), noExpire);
            hc.Session.Clear();
            hc.Session["u"] = u;
            hc.Session["p"] = 权限管理.计算权限(u);
            return u;
        }
        public static T 登录<T>(this HttpContextBase hc, string loginName, string password, bool noExpire) where T : 用户基本数据
        {
            T u;
            if (!用户管理.验证登录名和密码(loginName, password, out u)) return null;
            FormsAuthentication.SetAuthCookie(u.Id.ToString(), noExpire);
            hc.Session.Clear();
            hc.Session.Add("u", u);
            return u;
        }
        public static void 登出(this HttpContextBase hc)
        {
            var u = hc.Session["u"] as 用户基本数据;
            if (null != u)
            {
                lock (LoginSessions)
                {
                    if (LoginSessions.ContainsKey(u.Id))
                        LoginSessions.Remove(u.Id);
                }
            }
            hc.Session["u"] = null;
            hc.Session["p"] = null;
            var d = DateTime.Now.AddDays(-1);
            foreach (var item in hc.Response.Cookies.AllKeys)
            {
                hc.Response.Cookies[item].Expires = d;
                hc.Response.Cookies.Remove(item);
            }
            FormsAuthentication.SignOut();
        }
        public static 用户基本数据 获取当前用户(this HttpContextBase hc)
        {
            return hc.Session["u"] as 用户基本数据 ?? 游客.Default;
        }
        public static IEnumerable<string> 获取当前用户权限列表(this HttpContextBase hc)
        {
            return hc.Session["p"] as IEnumerable<string>;
        }
        public static T 获取当前用户<T>(this HttpContextBase hc) where T : 用户基本数据
        {
            var u = hc.Session["u"] as T;
            if (null != u) return u;
            hc.Response.RedirectToRoute(new System.Web.Routing.RouteValueDictionary() {
                { "action", "WrongUserType" },
                { "controller", "错误页面" },
            });
            return null;
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class 登录验证 : ActionFilterAttribute
    {
        public 登录验证() { Order = 100; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var hc = filterContext.HttpContext;
            var uid = hc.检查登录();
            if (-1 == uid) filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}
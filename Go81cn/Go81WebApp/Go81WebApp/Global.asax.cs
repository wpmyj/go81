using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.用户数据模型;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Go81WebApp
{
    public partial class WebApiApplication : HttpApplication
    {
        private const string HostName = "go81.cn";

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ThemedViewEngine());
        }

#if !DEBUG && !INTRANET
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //var host = Request.Url.Host;
            //if (host.Contains(HostName)) return;
            //var url = Request.Url.ToString().Trim().ToLower();
            //Response.RedirectPermanent(url.Replace(host, HostName));
        }
#endif

        protected void Session_Start(Object sender, EventArgs e)
        {
            Session.Timeout = 60 * 24;
        }

        protected void Session_End(Object sender, EventArgs e)
        {
            var u = Session["u"] as 用户基本数据;
            if (null == u) return;
            lock (登录管理.LoginSessions)
            {
                登录管理.LoginSessions.Remove(u.Id);
            }
        }
    }
    public partial class WebApiApplication
    {
        public static bool IsIntranet
        {
            get
            {
                return
#if INTRANET
 true
#else
                    false
#endif
;
            }
        }

        public static bool IsDebug
        {
            get
            {
                return
#if DEBUG
 true
#else
                    false
#endif
;
            }
        }
    }
}

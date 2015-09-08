using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Go81WebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            if(WebApiApplication.IsIntranet)
            {
                routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "首页", action = "Index", id = UrlParameter.Optional,IsIntranet=true }
            );
            }
            else
            {
                routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "首页", action = "Index", id = UrlParameter.Optional, IsIntranet = false }
            );
            }
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Index", action = "Index", id = UrlParameter.Optional }
            //);
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Index", action = "Index", id = UrlParameter.Optional }
            //);
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Index", action = "Index", id = UrlParameter.Optional }
            //);
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Index", action = "Index", id = UrlParameter.Optional }
            //);
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Index", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}

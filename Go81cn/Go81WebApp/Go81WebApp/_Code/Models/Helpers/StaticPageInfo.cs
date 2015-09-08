using System.IO;
using System.Web.Configuration;
using System.Web.Routing;

// ReSharper disable once CheckNamespace
namespace System.Web.Mvc
{
    public abstract class StaticPageInfoProvider
    {
        public string Dir { get; protected set; }
        public string FilePath { get; protected set; }
    }
    public class StaticPageInfo : StaticPageInfoProvider
    {
        private const string DefaultStaticPagesBaseDir = "~/静态化页面";
        private const string Extension = ".html";
        private static readonly string AppStaticPagesBaseDir = WebConfigurationManager.AppSettings["静态化页面目录"];
        static StaticPageInfo()
        {
            if (string.IsNullOrWhiteSpace(AppStaticPagesBaseDir))
                AppStaticPagesBaseDir = DefaultStaticPagesBaseDir;
        }
        public StaticPageInfo(HttpContextBase httpContext, RouteData routeData)
        {
            Dir = httpContext.Server.MapPath(
                string.Format(
                    "{1}{0}{2}{0}{3}",
                    Path.DirectorySeparatorChar,
                    AppStaticPagesBaseDir,
                    ThemeInfo.GetThemeName(httpContext),
                    routeData.Values["controller"]
                    ));
            var id = routeData.Values["id"];
            if (null != id) id = "-" + id;
            FilePath = string.Format("{1}{0}{2}{3}{4}", Path.DirectorySeparatorChar,
                Dir, routeData.Values["action"], id, Extension);
        }
    }
}
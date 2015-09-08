using System.Web;
using System.Web.Optimization;

namespace Go81WebApp
{
    public class BundleConfig
    {
        // 有关绑定的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.11.1.min.js",
                        "~/Scripts/jquery.validate.min.js",
                        "~/Scripts/jquery.validate.unobtrusive.min.js",
                        "~/Scripts/jquery.unobtrusive-ajax.min.js"
                        ));

            // 使用要用于开发和学习的 Modernizr 的开发版本。然后，当您做好
            // 生产准备时，请使用 http://modernizr.com 上的生成工具来仅选择所需的测试。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
#if INTRANET
            bundles.Add(new StyleBundle("~/css/css").Include(
                       "~/css/Intranet_p.css",
                        "~/css/jquery.autocomplete.css"));
#else
            bundles.Add(new StyleBundle("~/css/css").Include(
                        "~/css/Extranet.css",
                        "~/css/stylesheet.css",
                        "~/css/jquery.autocomplete_e.css"));
#endif
        }
    }
}

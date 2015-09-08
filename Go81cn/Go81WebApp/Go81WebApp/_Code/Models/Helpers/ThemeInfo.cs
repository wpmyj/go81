using Go81WebApp.Models.管理器;

// ReSharper disable once CheckNamespace
namespace System.Web.Mvc
{
    public abstract class ThemeInfoProvider
    {
        public string ThemeName { get; protected set; }
    }
    public class ThemeInfo : ThemeInfoProvider
    {
        private const string DefaultThemeName = "默认主题";
        private static readonly string AppDefaultThemeName = Configuration.WebConfigurationManager.AppSettings["默认主题"];
        public static string GetThemeName(HttpContextBase hc)
        {
            var themeName = hc.获取当前用户().登录信息.主题;
            return !string.IsNullOrWhiteSpace(themeName)
                ? themeName
                : !string.IsNullOrWhiteSpace(AppDefaultThemeName)
                    ? AppDefaultThemeName
                    : DefaultThemeName;
        }
        public ThemeInfo(HttpContextBase hc)
        {
            ThemeName = GetThemeName(hc);
        }
    }
}
using Go81WebApp.Models.数据模型.用户数据模型;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace System.Web.Mvc
{
    public class ThemedViewEngine : RazorViewEngine
    {
        public const char virtualPathSeparatorChar = '/';
        public static readonly string DefaultThemeName = ConfigurationManager.AppSettings["默认主题"];
        private static readonly string[] _emptyLocations = new string[0];
        internal Func<string, string> GetExtensionThunk = new Func<string, string>(VirtualPathUtility.GetExtension);
        public ThemedViewEngine()
        {
            // {0} - Action
            // {1} - Controller
            // {2} - Area
            // {3} - Controller Namespace : 必须以正斜杠结尾
            // {4} - Theme Name

            base.AreaViewLocationFormats = new string[] {
                "~/Areas/{2}/Views/{4}/{3}{1}/{0}.cshtml",
                //"~/Areas/{2}/Views/{4}/{3}{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/{4}/{3}_Shared/{0}.cshtml",
                //"~/Areas/{2}/Views/{4}/{3}_Shared/{0}.vbhtml",
                "~/Areas/{2}/Views/{4}/_Shared/{0}.cshtml",
                //"~/Areas/{2}/Views/{4}/_Shared/{0}.vbhtml",
            };
            base.AreaMasterLocationFormats = new string[] {
                "~/Areas/{2}/Views/{4}/{3}{1}/{0}.cshtml",
                //"~/Areas/{2}/Views/{4}/{3}{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/{4}/{3}_Shared/{0}.cshtml",
                //"~/Areas/{2}/Views/{4}/{3}_Shared/{0}.vbhtml",
                "~/Areas/{2}/Views/{4}/_Shared/{0}.cshtml",
                //"~/Areas/{2}/Views/{4}/_Shared/{0}.vbhtml",
            };
            base.AreaPartialViewLocationFormats = new string[] {
                "~/Areas/{2}/Views/{4}/{3}{1}/{0}.cshtml",
                //"~/Areas/{2}/Views/{4}/{3}{1}/{0}.vbhtml",
                "~/Areas/{2}/Views/{4}/{3}_Shared/{0}.cshtml",
                //"~/Areas/{2}/Views/{4}/{3}_Shared/{0}.vbhtml",
                "~/Areas/{2}/Views/{4}/_Shared/{0}.cshtml",
                //"~/Areas/{2}/Views/{4}/_Shared/{0}.vbhtml",
            };
            base.ViewLocationFormats = new string[] {
                "~/Views/{4}/{3}{1}/{0}.cshtml",
                //"~/Views/{4}/{3}{1}/{0}.vbhtml",
                "~/Views/{4}/{3}_Shared/{0}.cshtml",
                //"~/Views/{4}/{3}_Shared/{0}.vbhtml",
                "~/Views/{4}/_Shared/{0}.cshtml",
                //"~/Views/{4}/_Shared/{0}.vbhtml",
            };
            base.MasterLocationFormats = new string[] {
                "~/Views/{4}/{3}{1}/{0}.cshtml",
                //"~/Views/{4}/{3}{1}/{0}.vbhtml",
                "~/Views/{4}/{3}_Shared/{0}.cshtml",
                //"~/Views/{4}/{3}_Shared/{0}.vbhtml",
                "~/Views/{4}/_Shared/{0}.cshtml",
                //"~/Views/{4}/_Shared/{0}.vbhtml",
            };
            base.PartialViewLocationFormats = new string[] {
                "~/Views/{4}/{3}{1}/{0}.cshtml",
                //"~/Views/{4}/{3}{1}/{0}.vbhtml",
                "~/Views/{4}/{3}_Shared/{0}.cshtml",
                //"~/Views/{4}/{3}_Shared/{0}.vbhtml",
                "~/Views/{4}/_Shared/{0}.cshtml",
                //"~/Views/{4}/_Shared/{0}.vbhtml",
            };
            base.FileExtensions = new string[] {
                "cshtml",
                //"vbhtml",
            };
        }
        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            string[] searchedLocations;
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException("NullOrEmpty", "partialViewName");
            }
            string requiredString = controllerContext.RouteData.GetRequiredString("controller");
            string partialViewPath = this.GetPath(controllerContext, this.PartialViewLocationFormats, this.AreaPartialViewLocationFormats, "PartialViewLocationFormats", partialViewName, requiredString, "Partial", useCache, out searchedLocations);
            if (string.IsNullOrEmpty(partialViewPath))
            {
                return new ViewEngineResult(searchedLocations);
            }
            return new ViewEngineResult(this.CreatePartialView(controllerContext, partialViewPath), this);
        }
        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            string[] searchedViewLocations;
            string[] searchedMasterLocations;
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("NullOrEmpty", "viewName");
            }
            string requiredString = controllerContext.RouteData.GetRequiredString("controller");
            string viewPath = this.GetPath(controllerContext, this.ViewLocationFormats, this.AreaViewLocationFormats, "ViewLocationFormats", viewName, requiredString, "View", useCache, out searchedViewLocations);
            string masterPath = this.GetPath(controllerContext, this.MasterLocationFormats, this.AreaMasterLocationFormats, "MasterLocationFormats", masterName, requiredString, "Master", useCache, out searchedMasterLocations);
            if (!string.IsNullOrEmpty(viewPath) && (!string.IsNullOrEmpty(masterPath) || string.IsNullOrEmpty(masterName)))
            {
                return new ViewEngineResult(this.CreateView(controllerContext, viewPath, masterPath), this);
            }
            return new ViewEngineResult(searchedViewLocations.Union<string>(searchedMasterLocations));
        }
        private string GetControllerNamespace(ControllerContext controllerContext)
        {
            const string controllerNamespacePart = ".Controllers.";
            var controllerNamespace = controllerContext.Controller.GetType().Namespace;
            return controllerNamespace.Contains(controllerNamespacePart)
                ? controllerNamespace.Substring(
                    controllerNamespace.LastIndexOf(controllerNamespacePart) + controllerNamespacePart.Length
                    )
                : null
                ;
        }
        private string GetThemeName(ControllerContext controllerContext)
        {
            var u = controllerContext.HttpContext.Session["u"] as 用户基本数据;
            return (null == u || string.IsNullOrWhiteSpace(u.登录信息.主题))
                ? string.IsNullOrWhiteSpace(ThemedViewEngine.DefaultThemeName)
                    ? "默认主题"
                    : ThemedViewEngine.DefaultThemeName
                : u.登录信息.主题
                ;
        }
        private string GetPath(ControllerContext controllerContext, string[] locations, string[] areaLocations, string locationsPropertyName, string name, string controllerName, string cacheKeyPrefix, bool useCache, out string[] searchedLocations)
        {
            searchedLocations = _emptyLocations;
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            string areaName = Area.GetAreaName(controllerContext.RouteData);
            bool hasArea = !string.IsNullOrEmpty(areaName);
            List<ViewLocation> viewLocations = GetViewLocations(locations, hasArea ? areaLocations : null);
            if (viewLocations.Count == 0)
            {
                throw new InvalidOperationException(locationsPropertyName + ":PropertyCannotBeNullOrEmpty");
            }
            bool isSpecificPath = IsSpecificPath(name);
            string controllerNamespace = this.GetControllerNamespace(controllerContext);
            string themeName = this.GetThemeName(controllerContext);
            string cacheKey = this.CreateCacheKey(cacheKeyPrefix, name, isSpecificPath ? string.Empty : controllerName, areaName, controllerNamespace, themeName);
            if (useCache)
            {
                foreach (IDisplayMode mode in this.DisplayModeProvider.GetAvailableDisplayModesForContext(controllerContext.HttpContext, controllerContext.DisplayMode))
                {
                    string viewLocation = this.ViewLocationCache.GetViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, mode.DisplayModeId));
                    if (viewLocation == null)
                    {
                        return null;
                    }
                    if (viewLocation.Length > 0)
                    {
                        if (controllerContext.DisplayMode == null)
                        {
                            controllerContext.DisplayMode = mode;
                        }
                        return viewLocation;
                    }
                }
                return null;
            }
            return isSpecificPath
                ? this.GetPathFromSpecificName(controllerContext, name, cacheKey, ref searchedLocations)
                : this.GetPathFromGeneralName(controllerContext, viewLocations, name, controllerName, areaName, controllerNamespace, themeName, cacheKey, ref searchedLocations)
                ;
        }
        private string GetPathFromGeneralName(ControllerContext controllerContext, List<ViewLocation> locations, string name, string controllerName, string areaName, string controllerNamespace, string themeName, string cacheKey, ref string[] searchedLocations)
        {
            Func<string, bool> fileExist = path => this.FileExists(controllerContext, path);
            string virtualPath = string.Empty;
            searchedLocations = new string[locations.Count];
            for (int i = 0; i < locations.Count; i++)
            {
                string searchingLocation = locations[i].Format(
                    name,
                    controllerName,
                    areaName,
                    null == controllerNamespace ? string.Empty : (controllerNamespace.Replace('.', virtualPathSeparatorChar) + virtualPathSeparatorChar),
                    themeName
                    );
                DisplayInfo info = this.DisplayModeProvider.GetDisplayInfoForVirtualPath(searchingLocation, controllerContext.HttpContext, fileExist, controllerContext.DisplayMode);
                if (info != null)
                {
                    string filePath = info.FilePath;
                    searchedLocations = _emptyLocations;
                    virtualPath = filePath;
                    this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, info.DisplayMode.DisplayModeId), virtualPath);
                    if (controllerContext.DisplayMode == null)
                    {
                        controllerContext.DisplayMode = info.DisplayMode;
                    }
                    foreach (IDisplayMode mode in this.DisplayModeProvider.Modes)
                    {
                        if (mode.DisplayModeId != info.DisplayMode.DisplayModeId)
                        {
                            DisplayInfo info2 = mode.GetDisplayInfo(controllerContext.HttpContext, searchingLocation, fileExist);
                            string displayInfoVirtualPath = string.Empty;
                            if ((info2 != null) && (info2.FilePath != null))
                            {
                                displayInfoVirtualPath = info2.FilePath;
                            }
                            this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, mode.DisplayModeId), displayInfoVirtualPath);
                        }
                    }
                    return virtualPath;
                }
                searchedLocations[i] = searchingLocation;
            }
            return virtualPath;
        }
        private string GetPathFromSpecificName(ControllerContext controllerContext, string name, string cacheKey, ref string[] searchedLocations)
        {
            string virtualPath = name;
            if (!this.FilePathIsSupported(name) || !this.FileExists(controllerContext, name))
            {
                virtualPath = string.Empty;
                searchedLocations = new string[] { name };
            }
            this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
            return virtualPath;
        }
        private bool FilePathIsSupported(string virtualPath)
        {
            if (this.FileExtensions == null)
            {
                return true;
            }
            string str = this.GetExtensionThunk(virtualPath).TrimStart(new char[] { '.' });
            return this.FileExtensions.Contains<string>(str, StringComparer.OrdinalIgnoreCase);
        }
        internal virtual string CreateCacheKey(string prefix, string name, string controllerName, string areaName, string controllerNamespace, string themeName)
        {
            return string.Format(CultureInfo.InvariantCulture, ":ViewCacheEntry:{0}:{1}:{2}:{3}{5}{6}:{4}:", new object[] { base.GetType().AssemblyQualifiedName, prefix, name, controllerName, areaName, controllerNamespace, themeName });
        }
        private static bool IsSpecificPath(string name)
        {
            char ch = name[0];
            if (ch != '~')
            {
                return (ch == '/');
            }
            return true;
        }
        internal static string AppendDisplayModeToCacheKey(string cacheKey, string displayMode)
        {
            return (cacheKey + displayMode + ":");
        }
        private static List<ViewLocation> GetViewLocations(string[] viewLocationFormats, string[] areaViewLocationFormats)
        {
            List<ViewLocation> list = new List<ViewLocation>();
            if (areaViewLocationFormats != null)
            {
                foreach (string str in areaViewLocationFormats)
                {
                    list.Add(new AreaAwareViewLocation(str));
                }
            }
            if (viewLocationFormats != null)
            {
                foreach (string str2 in viewLocationFormats)
                {
                    list.Add(new ViewLocation(str2));
                }
            }
            return list;
        }
        private class ViewLocation
        {
            protected string _virtualPathFormatString;
            public ViewLocation(string virtualPathFormatString)
            {
                this._virtualPathFormatString = virtualPathFormatString;
            }

            public virtual string Format(string viewName, string controllerName, string areaName, string controllerNamespace, string themeName)
            {
                return string.Format(CultureInfo.InvariantCulture, this._virtualPathFormatString, new object[] { viewName, controllerName, areaName, controllerNamespace, themeName });
            }
        }
        private class AreaAwareViewLocation : ViewLocation
        {
            public AreaAwareViewLocation(string virtualPathFormatString)
                : base(virtualPathFormatString)
            {
            }
        }
        internal static class Area
        {
            public static string GetAreaName(RouteBase route)
            {
                IRouteWithArea area = route as IRouteWithArea;
                if (area != null)
                {
                    return area.Area;
                }
                Route route2 = route as Route;
                if ((route2 != null) && (route2.DataTokens != null))
                {
                    return (route2.DataTokens["area"] as string);
                }
                return null;
            }
            public static string GetAreaName(RouteData routeData)
            {
                object obj2;
                if (routeData.DataTokens.TryGetValue("area", out obj2))
                {
                    return (obj2 as string);
                }
                return GetAreaName(routeData.Route);
            }
        }
    }
}
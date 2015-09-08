using System.IO.Compression;

// ReSharper disable once CheckNamespace

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class GZipResponse : ActionFilterAttribute
    {
        public GZipResponse()
        {
            Order = 999999;
        }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var r = filterContext.HttpContext.Response;
            if (304 == r.StatusCode) return;

            var a = filterContext.HttpContext.Request.Headers["Accept-Encoding"];
            if (string.IsNullOrEmpty(a)) return;

            a = a.ToLower();
            if (a.Contains("gzip"))
            {
                r.AppendHeader("Content-encoding", "gzip");
                r.Filter = new GZipStream(r.Filter, CompressionMode.Compress);
            }
            else if (a.Contains("deflate"))
            {
                r.AppendHeader("Content-encoding", "deflate");
                r.Filter = new DeflateStream(r.Filter, CompressionMode.Compress);
            }
        }
    }
    [GZipResponse]
    public class GzipController : Controller { }
}
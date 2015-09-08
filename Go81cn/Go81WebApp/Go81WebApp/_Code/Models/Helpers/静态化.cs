using System.IO;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class 静态化 : ActionFilterAttribute
    {
#if DEBUG
        public 静态化(int expireMinutes = 0, int cacheSize = 0) { }
#else
        private static readonly TimedCacheProvider<string> StaticPagesCache = new TimedCache<string>();
        private const int DefaultStaticInterval = 5; //int.Parse(System.Configuration.ConfigurationManager.AppSettings["页面静态化更新时间"]);
        private const int DefaultCacheSize = 1024; //int.Parse(System.Configuration.ConfigurationManager.AppSettings["内存缓存页面最大长度"]);
        private StaticPageInfoProvider _staticPageInfo;
        private bool _staticed;
        private readonly int _expireMinutes;
        private readonly int _cacheSize;
        private readonly bool _http304;
        private readonly int _freshHour;
        private readonly int _freshMinute;

        /// <param name="expireMinutes">静态化页面过期时间（分钟）。
        /// -1 为永不过期，0 为使用配置文件默认值。</param>
        /// <param name="freshHour">每天定时更新静态化的时间(点)</param>
        /// <param name="freshMinute">每天定时更新静态化的时间(分钟)</param>
        /// <param name="cacheSize">进入内存缓存的静态化页面的最大长度（KB），超过该大小的文件将直接从磁盘读取。
        /// -1 为不使用内存缓存，0 为使用配置文件默认值，大于 0 为进入内存缓存的最大文件长度。</param>
        /// <param name="http304">是否使用HTTP 304本地缓存优化</param>
        public 静态化(int expireMinutes = 0, int freshHour = -1, ushort freshMinute = 0, int cacheSize = DefaultCacheSize, bool http304 = false)
        {
            Order = 500;
            _expireMinutes = 0 > expireMinutes
                ? -1
                : 0 == expireMinutes
                    ? DefaultStaticInterval
                    : expireMinutes
                ;
            _freshHour = 0 > freshHour
                ? -1
                : freshHour%24;
            _freshMinute = freshMinute%60;
            _cacheSize = 0 > cacheSize
                ? -1
                : 0 == cacheSize
                    ? DefaultCacheSize
                    : cacheSize
                ;
            _http304 = http304;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _staticPageInfo = new StaticPageInfo(filterContext.HttpContext, filterContext.RouteData);
            var fi = new FileInfo(_staticPageInfo.FilePath);
            var lwt = fi.LastWriteTime;
            var now = DateTime.Now;
            _staticed = fi.Exists && (
                (-1 != _freshHour && lwt.Day == now.Day &&
                 lwt.Hour*60 + lwt.Minute + lwt.Second > _freshHour*60 + _freshMinute)
                || - 1 == _expireMinutes //永不过期
                || now.Subtract(lwt).TotalMinutes < _expireMinutes //还未过期
                );
            if (!_staticed) return;
            filterContext.HttpContext.Response.Cache.SetLastModified(lwt);
            //HTTP 304
            DateTime d;
            if (_http304
                && DateTime.TryParse(filterContext.HttpContext.Request.Headers["If-Modified-Since"], out d)
                && Math.Floor(d.Subtract(lwt).TotalMinutes) + 1 < _expireMinutes)
            {
                filterContext.HttpContext.Response.StatusCode = 304;
                return;
            }
            //使用缓存静态化
            if (null != StaticPagesCache && -1 != _cacheSize)
            {
                if (fi.Length/1024 <= _cacheSize)
                {
                    //查缓存
                    filterContext.Result = new ContentResult
                    {
                        Content = StaticPagesCache.GetCacheOrUpdate(_staticPageInfo.FilePath,
                            lwt, () => File.ReadAllText(_staticPageInfo.FilePath)),
                        ContentType = "text/html",
                    };
                    return;
                }
                StaticPagesCache.Remove(_staticPageInfo.FilePath);
            }
            //不使用缓存静态化
            filterContext.Result = new FileStreamResult(fi.OpenRead(), "text/html");
        }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (_staticed) return;
            if (!Directory.Exists(_staticPageInfo.Dir)) Directory.CreateDirectory(_staticPageInfo.Dir);
            filterContext.HttpContext.Response.Filter = new HttpStaticFilter(filterContext.HttpContext.Response.Filter, _staticPageInfo.FilePath);
        }
        private class HttpStaticFilter : Stream
        {
            private readonly Stream _inner;
            private readonly FileStream _file;
            public HttpStaticFilter(Stream s, string fn, int secondsWait = 20)
            {
                _inner = s;
                var i = 0;
                while (File.Exists(fn) && i < 10 * secondsWait)
                {
                    try
                    {
                        File.Delete(fn);
                    }
                    catch
                    {
                        Thread.Sleep(100);
                        ++i;
                    }
                }
                _file = new FileStream(fn, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            public override bool CanRead { get { return _inner.CanRead; } }
            public override bool CanSeek { get { return _inner.CanSeek; } }
            public override bool CanWrite { get { return _inner.CanWrite; } }
            public override long Length { get { return _inner.Length; } }
            public override long Position { get { return _inner.Position; } set { _inner.Position = value; _file.Position = value; } }
            public override void Close()
            {
                _inner.Close();
                _file.Close();
            }
            public override void Flush()
            {
                _inner.Flush();
                _file.Flush();
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                var ret = _inner.Seek(offset, origin);
                _file.Seek(offset, origin);
                return ret;
            }
            public override void SetLength(long value)
            {
                _inner.SetLength(value);
                _file.SetLength(value);
            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                return _inner.Read(buffer, offset, count);
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                _inner.Write(buffer, offset, count);
                _file.Write(buffer, offset, count);
            }
        }
#endif
    }
}
using MongoDB.Bson;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Go81WebApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}"
                //routeTemplate: "api/{controller}/{action}/{id}",
                //defaults: new { id = RouteParameter.Optional }
            );
            config.Formatters.Clear();
            config.Formatters.Add(new TextJsonMediaTypeFormatter());

        }
        public class TextJsonMediaTypeFormatter : MediaTypeFormatter
        {
            private readonly Encoding _enc = Encoding.UTF8;
            public override bool CanReadType(Type type) { return true; }
            public override bool CanWriteType(Type type) { return true; }
            public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
            {
                headers.ContentType = new MediaTypeHeaderValue("text/json");
                headers.ContentEncoding.Add(_enc.WebName);
            }
            public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
                TransportContext transportContext, CancellationToken cancellationToken)
            {
                //var bom = _enc.GetPreamble();
                //if (0 != bom.Length) writeStream.Write(bom, 0, bom.Length);
                var buff = _enc.GetBytes(
                    value is string ? value.ToString() :
                    value.GetType().IsPrimitive ? value.ToString() :
                    value.ToJson(type));
                writeStream.Write(buff, 0, buff.Length);
                return Task.FromResult<byte>(0);
            }
        }
    }
}

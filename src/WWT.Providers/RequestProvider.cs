#nullable disable

using System.Threading;
using System.Threading.Tasks;

namespace WWT.Providers
{
    public abstract class RequestProvider
    {
        public abstract Task RunAsync(IWwtContext context, CancellationToken token);

        public virtual bool IsCacheable => true;

        public abstract string ContentType { get; }

        protected static class ContentTypes
        {
            public const string Png = "image/png";

            public const string Xml = "text/xml";

            public const string Jpeg = "image/jpeg";

            public const string OctetStream = "application/octet-stream";

            public const string Text = "text/plain";

            public const string Html = "text/html";

            public const string XWtt = "application/x-wtt";

            public const string XWtml = "application/x-wtml";

            public const string Zip = "application/zip";
        }

        protected Task Report304Async(IWwtContext context, CancellationToken token) {
            context.Response.StatusCode = 304;
            return context.Response.WriteAsync("HTTP/304 Not Modified", token);
        }

        protected Task Report404Async(IWwtContext context, string detail, CancellationToken token) {
            context.Response.StatusCode = 404;
            return context.Response.WriteAsync($"HTTP/404 Not Found\n\n{detail}", token);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using WWTWebservices;

namespace WWT.Providers
{
    public class DSSToastProvider : RequestProvider
    {
        private readonly IPlateTilePyramid _plateTile;
        private readonly FilePathOptions _options;

        public DSSToastProvider(IPlateTilePyramid plateTile, FilePathOptions options)
        {
            _plateTile = plateTile;
            _options = options;
        }

        public override Task RunAsync(IWwtContext context, CancellationToken token)
        {
            string query = context.Request.Params["Q"];
            string[] values = query.Split(',');
            int level = Convert.ToInt32(values[0]);
            int tileX = Convert.ToInt32(values[1]);
            int tileY = Convert.ToInt32(values[2]);

            if (level > 12)
            {
                context.Response.Write("No image");
                context.Response.Close();
            }
            else if (level < 8)
            {
                context.Response.ContentType = "image/png";

                using (var s = _plateTile.GetStream(_options.WwtTilesDir, "DSSToast.plate", level, tileX, tileY))
                {
                    s.CopyTo(context.Response.OutputStream);
                    context.Response.Flush();
                    context.Response.End();
                }
            }
            else
            {
                int powLev5Diff = (int)Math.Pow(2, level - 5);
                int X32 = tileX / powLev5Diff;
                int Y32 = tileY / powLev5Diff;

                int L5 = level - 5;
                int X5 = tileX % powLev5Diff;
                int Y5 = tileY % powLev5Diff;

                context.Response.ContentType = "image/png";

                var filename = $"DSSpngL5to12_x{X32}_y{Y32}.plate";

                using (var s = _plateTile.GetStream(_options.DssToastPng, filename, L5, X5, Y5))
                {
                    s.CopyTo(context.Response.OutputStream);
                    context.Response.Flush();
                    context.Response.End();
                }
            }

            return Task.CompletedTask;
        }
    }
}
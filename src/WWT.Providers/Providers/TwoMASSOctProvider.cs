﻿using System;
using System.Configuration;
using System.IO;
using WWTWebservices;

namespace WWT.Providers
{
    public class TwoMASSOctProvider : RequestProvider
    {
        private readonly IPlateTilePyramid _plateTiles;
        private readonly FilePathOptions _options;

        public TwoMASSOctProvider(IPlateTilePyramid plateTiles, FilePathOptions options)
        {
            _plateTiles = plateTiles;
            _options = options;
        }

        public override void Run(IWwtContext context)
        {
            string query = context.Request.Params["Q"];
            string[] values = query.Split(',');
            int level = Convert.ToInt32(values[0]);
            int tileX = Convert.ToInt32(values[1]);
            int tileY = Convert.ToInt32(values[2]);

            if (level < 8)
            {
                context.Response.ContentType = "image/png";

                using (Stream s = _plateTiles.GetStream(_options.WwtTilesDir, "2massoctset.plate", level, tileX, tileY))
                {
                    s.CopyTo(context.Response.OutputStream);
                    context.Response.Flush();
                    context.Response.End();
                }
            }
        }
    }
}

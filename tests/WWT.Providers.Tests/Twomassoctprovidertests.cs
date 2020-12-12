﻿using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

using WWT.PlateFiles;

namespace WWT.Providers.Tests
{
    public class TwoMASSOctProviderTests : ProviderTests<TwoMASSOctProvider>
    {
        protected override int MaxLevel => 7;

        protected override Action<IResponse> NullStreamResponseHandler => null;

        protected override Action<IResponse> StreamExceptionResponseHandler => null;

        protected override void ExpectedResponseAboveMaxLevel(IResponse response)
        {
            Assert.Empty(response.ContentType);
            Assert.Empty(response.OutputStream.ToArray());
        }

        protected override Task<Stream> GetStreamFromPlateTilePyramidAsync(IPlateTilePyramid plateTiles, int level, int x, int y)
            => plateTiles.GetStreamAsync(Options.WwtTilesDir, $"2massoctset.plate", level, x, y, default);
    }
}

﻿using System;
using AutoFixture;
using System.IO;
using WWTWebservices;
using Xunit;

namespace WWT.Providers.Tests
{
    public class TilesProviderTests : ProviderTests<TilesProvider>
    {
        private readonly string _fileName;

        public TilesProviderTests()
        {
            _fileName = Fixture.Create<string>();
        }

        protected override object[] GetParameterQ(int level, int x, int y)
            => new object[] { level, x, y, _fileName };

        protected override int MaxLevel => 7;

        protected override Stream GetStreamFromPlateTilePyramid(IPlateTilePyramid plateTiles, int level, int x, int y)
            => plateTiles.GetStream(Options.WwtTilesDir, $"{_fileName}.plate", level, x, y);

        protected override Action<IResponse> NullStreamResponseHandler => null;

        protected override Action<IResponse> StreamExceptionResponseHandler => null;

        protected override void ExpectedResponseAboveMaxLevel(IResponse response)
        {
            Assert.Empty(response.ContentType);
            Assert.Empty(response.OutputStream.ToArray());
        }
    }
}

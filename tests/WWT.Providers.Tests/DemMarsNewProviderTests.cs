﻿using AutofacContrib.NSubstitute;
using AutoFixture;
using NSubstitute;
using NSubstitute.Extensions;
using System.IO;
using System.Linq;
using System.Web;
using WWTWebservices;
using Xunit;

namespace WWT.Providers.Tests
{
    public class DemMarsNewProviderTests
    {
        private const string Prefix = @"\\wwt-mars\marsroot\dem\";

        private readonly Fixture _fixture;

        public DemMarsNewProviderTests()
        {
            _fixture = new Fixture();
        }

        [InlineData(0, 0, 0)]
        [Theory]
        public void ExpectedTests(int level, int x, int y)
        {
            // Arrange
            var data = _fixture.CreateMany<byte>().ToArray();
            var hash = DirectoryEntry.ComputeHash(level + 128, x, y) % 400;
            using var container = AutoSubstitute.Configure()
                .InitializeProviderTests()
                .ConfigureParameterQ(level, x, y)
                .Build();
            container.Resolve<IPlateTilePyramid>().GetStream(Prefix, $"marsToastDem_{hash}.plate", -1, level, x, y).Returns(new MemoryStream(data));

            // Act
            container.RunProviderTest<DemMarsNewProvider>();

            // Assert
            container.Resolve<IPlateTilePyramid>().Received(1).GetStream(Prefix, $"marsToastDem_{hash}.plate", -1, level, x, y);
            Assert.Equal(data, container.GetOutputData());
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void EmptyResult(bool isNull)
        {
            // Arrange
            var level = _fixture.Create<int>() % 18;
            var x = _fixture.Create<int>();
            var y = _fixture.Create<int>();
            var hash = DirectoryEntry.ComputeHash(level + 128, x, y) % 400;

            using var container = AutoSubstitute.Configure()
                .InitializeProviderTests()
                .ConfigureParameterQ(level, x, y)
                .Build();

            if (isNull)
            {
                container.Resolve<IPlateTilePyramid>().GetStream(Prefix, $"marsToastDem_{hash}.plate", -1, level, x, y).Returns((Stream)null);
            }
            {
                var empty = Substitute.ForPartsOf<Stream>();
                empty.Configure().Length.Returns(0);
                container.Resolve<IPlateTilePyramid>().GetStream(Prefix, $"marsToastDem_{hash}.plate", -1, level, x, y).Returns(empty);
            }

            // Act
            container.RunProviderTest<DemMarsNewProvider>();

            // Assert
            container.Resolve<IPlateTilePyramid>().Received(1).GetStream(Prefix, $"marsToastDem_{hash}.plate", -1, level, x, y);
            container.Resolve<IResponse>().Received(1).Write("No image");
            Assert.Equal("text/plain", container.Resolve<IResponse>().ContentType);
        }
    }
}
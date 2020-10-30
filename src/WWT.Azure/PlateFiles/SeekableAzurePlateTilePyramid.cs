using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using WWTWebservices;

namespace WWT.Azure
{
    public class SeekableAzurePlateTilePyramid : IPlateTilePyramid
    {
        private readonly Func<string, BlobClient> _blobRetriever;
        private readonly BlobContainerClient _container;

        protected readonly ILogger<SeekableAzurePlateTilePyramid> _logger;

        public SeekableAzurePlateTilePyramid(AzurePlateTilePyramidOptions options, BlobServiceClient service, ILogger<SeekableAzurePlateTilePyramid> logger)
        {
            _container = service.GetBlobContainerClient(options.Container);
            _logger = logger;

            var cache = new ConcurrentDictionary<string, BlobClient>();
            _blobRetriever = plateName => cache.GetOrAdd(plateName, _container.GetBlobClient);
        }

        public Stream GetStream(string pathPrefix, string plateName, int level, int x, int y)
        {
            var client = GetBlob(pathPrefix, plateName);

            try
            {
                var download = client.OpenRead();

                return PlateTilePyramid.GetImageStream(download, level, x, y);
            }
            catch (RequestFailedException e)
            {
                _logger.LogError(e, "Unexpected error downloading {PlateName}", plateName);
                return null;
            }
        }

        public async IAsyncEnumerable<string> GetPlateNames([EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in _container.GetBlobsByHierarchyAsync(delimiter: "/", cancellationToken: token))
            {
                var prefix = item.Prefix.TrimEnd('/');
                yield return $"{prefix}.plate";
            }
        }

        public Stream GetStream(string pathPrefix, string plateName, int tag, int level, int x, int y)
        {
            var client = GetBlob(pathPrefix, plateName);

            try
            {
                var stream = client.OpenRead();

                return PlateFile2.GetImageStream(stream, tag, level, x, y);
            }
            catch (RequestFailedException e)
            {
                _logger.LogError(e, "Unexpected error downloading {PlateName}", plateName);
                return null;
            }
        }

        protected virtual BlobClient GetBlob(string pathPrefix, string plateName)
             => _blobRetriever(plateName);
    }
}
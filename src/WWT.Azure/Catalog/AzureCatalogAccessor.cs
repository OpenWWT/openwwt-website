﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WWT.Catalog;

namespace WWT.Azure.Catalog
{
    public class AzureCatalogAccessor : ICatalogAccessor
    {
        private readonly BlobContainerClient _container;

        public AzureCatalogAccessor(AzureCatalogOptions options, BlobServiceClient service)
        {
            _container = service.GetBlobContainerClient(options.ContainerName);
        }

        /// <summary>
        /// Catalog Entry will supply last accessed date and access to the blob stream
        /// </summary>
        /// <param name="catalogEntryName">The name of the catalog entry to fetch, this will be lower cased</param>
        public async Task<CatalogEntry> GetCatalogEntryAsync(string catalogEntryName, CancellationToken token)
        {
            if (catalogEntryName is null) 
                return null;

            var blob = _container.GetBlobClient(catalogEntryName.ToLower());

            if (blob is null || !blob.Exists())
                return null;

            BlobProperties properties = await blob.GetPropertiesAsync(cancellationToken:token);

            var entry = new CatalogEntry
            {
                Contents = await blob.OpenReadAsync(cancellationToken:token),
                LastModified = properties.LastModified.UtcDateTime
            };

            return entry;
        }
    }
}

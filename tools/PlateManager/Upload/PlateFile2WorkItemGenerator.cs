﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WWT.Azure;
using WWTWebservices;

namespace PlateManager
{
    internal sealed class PlateFile2WorkItemGenerator : PlateFileWorkItemGeneratorBase, IWorkItemGenerator
    {
        private readonly AzurePlateTilePyramid _pyramid;
        private readonly ILogger<PlateFile2WorkItemGenerator> _logger;

        public PlateFile2WorkItemGenerator(AzurePlateTilePyramid pyramid, ILogger<PlateFile2WorkItemGenerator> logger)
        {
            _pyramid = pyramid;
            _logger = logger;
        }

        public IEnumerable<Func<int, int, CancellationToken, Task>> GenerateWorkItems(string plateFile, string baseUrl)
        {
            var filepart = Path.GetFileNameWithoutExtension(plateFile);
            var azureContainer = Path.GetFileName(plateFile).ToLowerInvariant();

            // Handle thumbnails if one exists alongside the plate file
            string thumbnail = GetThumbnailName(plateFile);
            if (File.Exists(thumbnail))
            {
                _logger.LogTrace("Adding task for thumbnail {Path}", thumbnail);
                Task UploadThumbnail(int count, int total, CancellationToken token) => _pyramid.SaveStreamAsync(GetFileStream(thumbnail), azureContainer, GetThumbnailBlobName(filepart), token);
                yield return UploadThumbnail;
            }

            // Re-write entries in the WTML file to point to the new location for the images
            // to point to Azure if the WTML file exists alongside the plate file
            string wtmlfile = GetWtmlName(plateFile);
            if (File.Exists(wtmlfile))
            {
                string wtmlFileOut = wtmlfile.Replace(".wtml", ".azure.wtml");
                string wtmldata = UpdateWtmlEntries(File.ReadAllText(wtmlfile), filepart, baseUrl, azureContainer);
                File.WriteAllText(wtmlFileOut, wtmldata);
            }

            var plateFile2 = new PlateFile2(plateFile);
            var directoryEntries = plateFile2.GetEntries().ToList();

            _logger.LogInformation("Found {Count} files encoded in {File}", directoryEntries.Count, plateFile);
            foreach (var item in directoryEntries)
            {
                async Task UploadItem(int count, int total, CancellationToken token)
                {
                    _logger.LogTrace("[{Count} of {Total}] Starting upload for {File} {tag}/L{Level}X{X}Y{Y}", count, total, plateFile, item.tag, item.level, item.x, item.y);
                    try
                    {
                        var result = await ProcessPlateTileAsync(plateFile2, azureContainer, item.tag, item.level, item.x, item.y, token);

                        if (result)
                        {
                            _logger.LogInformation("[{Count} of {Total}] Completed upload for {File} {tag}/L{Level}X{X}Y{Y}", count, total, plateFile, item.tag, item.level, item.x, item.y);
                        }
                        else
                        {
                            _logger.LogWarning("[{Count} of {Total}] Skipped upload for {File} {tag}/L{Level}X{X}Y{Y}", count, total, plateFile, item.tag, item.level, item.x, item.y);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[{Count} of {Total}] Unexpected error uploading {File} {tag}/L{Level}X{X}Y{Y}", count, total, plateFile, item.tag, item.level, item.x, item.y);
                    }
                }
                yield return UploadItem;
            }
            _logger.LogInformation("Done adding upload tasks for {File}", plateFile);
        }

        private async Task<bool> ProcessPlateTileAsync(PlateFile2 plateFile, string container, int tag, int level, int x, int y, CancellationToken token)
        {
            using var stream = plateFile.GetFileStream(tag, level, x, y);

            if (stream != null)
            {
                return await _pyramid.SaveStreamAsync(stream, container, tag, level, x, y, token);
            }

            return false;
        }
    }
}

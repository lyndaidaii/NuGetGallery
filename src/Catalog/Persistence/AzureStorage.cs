﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using NuGet.Protocol;

namespace NuGet.Services.Metadata.Catalog.Persistence
{
    public class AzureStorage : Storage, IAzureStorage
    {
        private readonly bool _compressContent;
        private readonly IThrottle _throttle;
        private readonly BlobContainerClient _containerClient;
        private readonly string _pathPrefix;
        private readonly bool _useServerSideCopy;

        public const string Sha512HashAlgorithmId = "SHA512";
        public static readonly TimeSpan DefaultServerTimeout = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan DefaultMaxExecutionTime = TimeSpan.FromMinutes(10);

        public AzureStorage(
            BlobContainerClient blobContainerClient,
            string path,
            Uri baseAddress,
            TimeSpan maxExecutionTime,
            TimeSpan serverTimeout,
            bool useServerSideCopy,
            bool compressContent,
            bool verbose,
            bool initializeContainer,
            IThrottle throttle) : this(
                blobContainerClient,
                path,
                baseAddress,
                maxExecutionTime,
                serverTimeout,
                initializeContainer)
        {
            _useServerSideCopy = useServerSideCopy;
            _compressContent = compressContent;
            _throttle = throttle ?? NullThrottle.Instance;
            Verbose = verbose;

            if (initializeContainer)
            {
                _containerClient.CreateIfNotExists(PublicAccessType.Blob);
                if (Verbose)
                {
                    Trace.WriteLine($"Created '{_containerClient.Name}' public container");
                }
            }
        }

        public AzureStorage(
            Uri storageBaseUri,
            TimeSpan maxExecutionTime,
            TimeSpan serverTimeout,
            bool useServerSideCopy,
            bool compressContent,
            bool verbose,
            IThrottle throttle)
            : this(GetBlobContainerClientUri(storageBaseUri), GetPathPrefix(storageBaseUri), storageBaseUri, maxExecutionTime, serverTimeout, false)
        {
            _useServerSideCopy = useServerSideCopy;
            _compressContent = compressContent;
            _throttle = throttle ?? NullThrottle.Instance;
            Verbose = verbose;
        }

        private static BlobContainerClient GetBlobContainerClientUri(Uri storageBaseUri)
        {
            if (storageBaseUri.AbsoluteUri.Contains('%'))
            {
                // Later in the code for the sake of simplicity wrong things are done with URL that 
                // can explode when URL is specially crafted with certain URL-encoded characters.
                // Since it is URL for our storage root where we know that we don't use anything
                // that requires URL-encoding, we'll just throw here just in case, to keep code
                // below simple.
                throw new ArgumentException("Storage URL cannot contain URL-encoded characters");
            }

            var pathSegments = storageBaseUri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Length < 1)
            {
                throw new ArgumentException("Storage URL must contain some path");
            }

            var containerName = pathSegments[0];
            var serviceClient = new BlobServiceClient(storageBaseUri.GetLeftPart(UriPartial.Authority));
            return serviceClient.GetBlobContainerClient(containerName);
        }

        private static string GetPathPrefix(Uri storageBaseUri)
        {
            var pathSegments = storageBaseUri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("/", pathSegments.Skip(1));
        }

        public AzureStorage(
            BlobContainerClient containerClient,
            string pathPrefix,
            Uri baseAddress,
            TimeSpan maxExecutionTime,
            TimeSpan serverTimeout,
            bool initializeContainer) : base(
                baseAddress ?? new Uri(containerClient.Uri, pathPrefix))
        {
            _containerClient = containerClient;
            _pathPrefix = pathPrefix;

            // Set default request conditions for the container client
            // Note: Retry and timeout settings should be applied individually as needed in the new SDK
        }

        public override async Task<OptimisticConcurrencyControlToken> GetOptimisticConcurrencyControlTokenAsync(
            Uri resourceUri,
            CancellationToken cancellationToken)
        {
            if (resourceUri == null)
            {
                throw new ArgumentNullException(nameof(resourceUri));
            }

            cancellationToken.ThrowIfCancellationRequested();

            string blobName = GetName(resourceUri);
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            BlobProperties properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            return new OptimisticConcurrencyControlToken(properties.ETag.ToString());
        }

        // Blob exists
        public override bool Exists(string fileName)
        {
            Uri packageRegistrationUri = ResolveUri(fileName);
            string blobName = GetName(packageRegistrationUri);

            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            return blobClient.Exists();
        }

        public override async Task<IEnumerable<StorageListItem>> ListAsync(CancellationToken cancellationToken)
        {
            var blobs = new List<StorageListItem>();

            await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: _pathPrefix, cancellationToken: cancellationToken))
            {
                var lastModified = blobItem.Properties.LastModified?.UtcDateTime;
                blobs.Add(new StorageListItem(new Uri(_containerClient.Uri, blobItem.Name), lastModified));
            }

            return blobs;
        }

        public override async Task<bool> UpdateCacheControlAsync(Uri resourceUri, string cacheControl, CancellationToken cancellationToken)
        {
            string blobName = GetName(resourceUri);
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            BlobProperties properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            if (properties.CacheControl != cacheControl)
            {
                var headers = new BlobHttpHeaders { CacheControl = cacheControl };
                await blobClient.SetHttpHeadersAsync(headers, cancellationToken: cancellationToken);
                return true;
            }

            return false;
        }

        protected override async Task OnCopyAsync(
            Uri sourceUri,
            IStorage destinationStorage,
            Uri destinationUri,
            IReadOnlyDictionary<string, string> destinationProperties,
            CancellationToken cancellationToken)
        {
            if (destinationStorage is not AzureStorage azureDestinationStorage)
            {
                throw new NotImplementedException("Copying is only supported from Azure storage to Azure storage.");
            }

            string sourceName = GetName(sourceUri);
            string destinationName = azureDestinationStorage.GetName(destinationUri);

            BlobClient sourceBlob = _containerClient.GetBlobClient(sourceName);
            BlobClient destinationBlob = azureDestinationStorage._containerClient.GetBlobClient(destinationName);

            var sourceUriWithSas = sourceBlob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));

            await destinationBlob.StartCopyFromUriAsync(sourceUriWithSas, cancellationToken: cancellationToken);

            if (destinationProperties?.Count > 0)
            {
                var headers = new BlobHttpHeaders();

                foreach (var property in destinationProperties)
                {
                    switch (property.Key)
                    {
                        case StorageConstants.CacheControl:
                            headers.CacheControl = property.Value;
                            break;
                        case StorageConstants.ContentType:
                            headers.ContentType = property.Value;
                            break;
                        default:
                            throw new NotImplementedException($"Storage property '{property.Key}' is not supported.");
                    }
                }

                await destinationBlob.SetHttpHeadersAsync(headers, cancellationToken: cancellationToken);
            }
        }

        protected override async Task OnSaveAsync(Uri resourceUri, StorageContent content, CancellationToken cancellationToken)
        {
            string name = GetName(resourceUri);
            BlobClient blobClient = _containerClient.GetBlobClient(name);

            var headers = new BlobHttpHeaders
            {
                ContentType = content.ContentType,
                CacheControl = content.CacheControl
            };

            if (_compressContent)
            {
                headers.ContentEncoding = "gzip";
                using (Stream stream = content.GetContentStream())
                {
                    MemoryStream destinationStream = new MemoryStream();

                    using (GZipStream compressionStream = new GZipStream(destinationStream, CompressionMode.Compress, true))
                    {
                        await stream.CopyToAsync(compressionStream);
                    }

                    destinationStream.Seek(0, SeekOrigin.Begin);

                    await blobClient.UploadAsync(destinationStream, headers, cancellationToken: cancellationToken);

                    Trace.WriteLine($"Saved compressed blob {blobClient.Uri} to container {_containerClient.Name}");
                }
            }
            else
            {
                using (Stream stream = content.GetContentStream())
                {
                    await blobClient.UploadAsync(stream, headers, cancellationToken: cancellationToken);

                    Trace.WriteLine($"Saved uncompressed blob {blobClient.Uri} to container {_containerClient.Name}");
                }
            }

            // Taking snapshots is not directly supported in the new SDK, you might need to implement your logic here.
        }

        protected override async Task<StorageContent> OnLoadAsync(Uri resourceUri, CancellationToken cancellationToken)
        {
            string name = GetName(resourceUri).TrimStart('/');
            BlobClient blobClient = _containerClient.GetBlobClient(name);

            await _throttle.WaitAsync();
            try
            {
                var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

                string content;
                using (var originalStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(originalStream, cancellationToken);

                    originalStream.Seek(0, SeekOrigin.Begin);

                    if (properties.Value.ContentEncoding == "gzip")
                    {
                        using (var uncompressedStream = new GZipStream(originalStream, CompressionMode.Decompress))
                        {
                            using (var reader = new StreamReader(uncompressedStream))
                            {
                                content = await reader.ReadToEndAsync();
                            }
                        }
                    }
                    else
                    {
                        using (var reader = new StreamReader(originalStream))
                        {
                            content = await reader.ReadToEndAsync();
                        }
                    }
                }

                return new StringStorageContentWithETag(content, properties.Value.ETag.ToString());
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                if (Verbose)
                {
                    Trace.WriteLine($"Can't load '{resourceUri}'. Blob doesn't exist");
                }

                return null;
            }
            finally
            {
                _throttle.Release();
            }
        }

        protected override async Task OnDeleteAsync(Uri resourceUri, DeleteRequestOptions deleteRequestOptions, CancellationToken cancellationToken)
        {
            string name = GetName(resourceUri);
            BlobRequestConditions accessCondition = (deleteRequestOptions as DeleteRequestOptionsWithAccessCondition)?.BlobRequestConditions;
            BlobClient blobClient = _containerClient.GetBlobClient(name);
            await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, accessCondition, cancellationToken);
        }

        public override Uri GetUri(string name)
        {
            var baseUri = _containerClient.Uri.AbsoluteUri;

            if (baseUri.EndsWith("/"))
            {
                return new Uri($"{baseUri}{name}", UriKind.Absolute);
            }

            return new Uri($"{baseUri}/{name}", UriKind.Absolute);
        }

        public override async Task<bool> AreSynchronized(Uri firstResourceUri, Uri secondResourceUri)
        {
            var sourceBlobClient = new BlobClient(firstResourceUri);
            var destinationBlobClient = _containerClient.GetBlobClient(GetName(secondResourceUri));

            return await AreSynchronized(new AzureCloudBlockBlob(sourceBlobClient), new AzureCloudBlockBlob(destinationBlobClient));
        }

        public async Task<bool> AreSynchronized(ICloudBlockBlob sourceBlockBlob, ICloudBlockBlob destinationBlockBlob)
        {
            if (await destinationBlockBlob.ExistsAsync(CancellationToken.None))
            {
                if (await sourceBlockBlob.ExistsAsync(CancellationToken.None))
                {
                    var sourceBlobMetadata = await sourceBlockBlob.GetMetadataAsync(CancellationToken.None);
                    var destinationBlobMetadata = await destinationBlockBlob.GetMetadataAsync(CancellationToken.None);
                    if (sourceBlobMetadata == null || destinationBlobMetadata == null)
                    {
                        return false;
                    }

                    var sourceBlobHasSha512Hash = sourceBlobMetadata.TryGetValue(Sha512HashAlgorithmId, out var sourceBlobSha512Hash);
                    var destinationBlobHasSha512Hash = destinationBlobMetadata.TryGetValue(Sha512HashAlgorithmId, out var destinationBlobSha512Hash);
                    if (!sourceBlobHasSha512Hash)
                    {
                        Trace.TraceWarning($"The source blob ({sourceBlockBlob.Uri}) doesn't have the SHA512 hash.");
                    }
                    if (!destinationBlobHasSha512Hash)
                    {
                        Trace.TraceWarning($"The destination blob ({destinationBlockBlob.Uri}) doesn't have the SHA512 hash.");
                    }
                    if (sourceBlobHasSha512Hash && destinationBlobHasSha512Hash)
                    {
                        if (sourceBlobSha512Hash == destinationBlobSha512Hash)
                        {
                            Trace.WriteLine($"The source blob ({sourceBlockBlob.Uri}) and destination blob ({destinationBlockBlob.Uri}) have the same SHA512 hash and are synchronized.");
                            return true;
                        }

                        // The SHA512 hash between the source and destination blob should be always same.
                        Trace.TraceWarning($"The source blob ({sourceBlockBlob.Uri}) and destination blob ({destinationBlockBlob.Uri}) have the different SHA512 hash and are not synchronized. " +
                            $"The source blob hash is {sourceBlobSha512Hash} while the destination blob hash is {destinationBlobSha512Hash}");
                    }

                    return false;
                }
                return true;
            }
            return !(await sourceBlockBlob.ExistsAsync(CancellationToken.None));
        }

        public async Task<ICloudBlockBlob> GetCloudBlockBlobReferenceAsync(Uri blobUri)
        {
            string blobName = GetName(blobUri);
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            var blobExists = await blobClient.ExistsAsync();

            if (Verbose && !blobExists)
            {
                Trace.WriteLine($"The blob {blobUri.AbsoluteUri} does not exist.");
            }

            return new AzureCloudBlockBlob(blobClient);
        }

        public async Task<bool> HasPropertiesAsync(Uri blobUri, string contentType, string cacheControl)
        {
            var blobName = GetName(blobUri);
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                var properties = await blobClient.GetPropertiesAsync();

                return string.Equals(properties.Value.ContentType, contentType)
                    && string.Equals(properties.Value.CacheControl, cacheControl);
            }

            return false;
        }
    }
}

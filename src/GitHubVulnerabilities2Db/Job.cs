﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using GitHubVulnerability2Db.Collector;
using GitHubVulnerability2Db.Configuration;
using GitHubVulnerability2Db.Gallery;
using GitHubVulnerability2Db.GraphQL;
using GitHubVulnerability2Db.Ingest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using NuGet.Jobs;
using NuGet.Jobs.Configuration;
using NuGet.Services.Cursor;
using NuGetGallery;
using NuGetGallery.Auditing;
using NuGetGallery.Security;

namespace GitHubVulnerability2Db
{
    public class Job : JsonConfigurationJob, IDisposable
    {
        private readonly HttpClient _client = new HttpClient();

        public override Task Run()
        {
            var collector = _serviceProvider.GetRequiredService<IVulnerabilityCollector>();
            using (var tokenSource = new CancellationTokenSource())
            {
                return collector.ProcessNewVulnerabilities(tokenSource.Token);
            }
        }

        protected override void ConfigureAutofacServices(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAdapter<IOptionsSnapshot<InitializationConfiguration>, InitializationConfiguration>(c => c.Value);

            ConfigureQueryServices(containerBuilder);
            ConfigureIngestionServices(containerBuilder);
            ConfigureCollectorServices(containerBuilder);
        }

        protected void ConfigureIngestionServices(ContainerBuilder containerBuilder)
        {
            ConfigureGalleryServices(containerBuilder);

            containerBuilder
                .RegisterType<PackageVulnerabilityService>()
                .As<IPackageVulnerabilityService>();

            containerBuilder
                .RegisterType<VulnerabilityIngestor>()
                .As<IVulnerabilityIngestor>();
        }

        protected void ConfigureGalleryServices(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .Register(ctx =>
                {
                    var connection = CreateSqlConnection<GalleryDbConfiguration>();
                    return new EntitiesContext(connection, false);
                })
                .As<IEntitiesContext>();

            containerBuilder
                .RegisterGeneric(typeof(EntityRepository<>))
                .As(typeof(IEntityRepository<>));

            containerBuilder
                .RegisterType<ThrowingAuditingService>()
                .As<IAuditingService>();

            containerBuilder
                .RegisterType<ThrowingTelemetryService>()
                .As<ITelemetryService>();

            containerBuilder
                .RegisterType<ThrowingSecurityPolicyService>()
                .As<ISecurityPolicyService>();

            containerBuilder
                .RegisterType<PackageService>()
                .As<IPackageService>();

            containerBuilder
                .RegisterType<ThrowingIndexingService>()
                .As<IIndexingService>();

            containerBuilder
                .RegisterType<PackageUpdateService>()
                .As<IPackageUpdateService>();
        }

        protected void ConfigureQueryServices(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterInstance(_client)
                .As<HttpClient>();

            containerBuilder
                .RegisterType<QueryService>()
                .As<IQueryService>();

            containerBuilder
                .RegisterType<VulnerabilityQueryService>()
                .As<IVulnerabilityQueryService>();
        }

        protected void ConfigureCollectorServices(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .Register(ctx =>
                {
                    var config = ctx.Resolve<IOptionsSnapshot<InitializationConfiguration>>().Value;
                    return CloudStorageAccount.Parse(config.StorageConnectionString);
                })
                .As<CloudStorageAccount>();

            containerBuilder
                .Register(ctx =>
                {
                    var config = ctx.Resolve<IOptionsSnapshot<InitializationConfiguration>>().Value;
                    var storageAccount = ctx.Resolve<CloudStorageAccount>();
                    var blob = storageAccount
                        .CreateCloudBlobClient()
                        .GetContainerReference(config.CursorContainerName)
                        .GetBlockBlobReference(config.CursorBlobName);

                    return new DurableStringCursor(blob);
                })
                .As<ReadWriteCursor<string>>();

            containerBuilder
                .RegisterType<VulnerabilityCollector>()
                .As<IVulnerabilityCollector>();
        }

        protected override void ConfigureJobServices(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            ConfigureInitializationSection<InitializationConfiguration>(services, configurationRoot);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuGet.Services.Metadata.Catalog.Monitoring
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterValidatorConfiguration(
            this ContainerBuilder builder,
            ValidatorConfiguration validatorConfig)
        {
            if (validatorConfig == null)
            {
                throw new ArgumentNullException(nameof(validatorConfig));
            }

            builder
                .RegisterInstance(validatorConfig)
                .AsSelf();
        }

        public static void RegisterEndpointConfiguration(
            this ContainerBuilder builder,
            EndpointConfiguration endpointConfig)
        {
            if (endpointConfig == null)
            {
                throw new ArgumentNullException(nameof(endpointConfig));
            }

            builder
                .RegisterInstance(endpointConfig)
                .AsSelf();
        }

        public static void RegisterMessageHandlerFactory(
            this ContainerBuilder builder,
            Func<HttpMessageHandler> messageHandlerFactory)
        {
            if (messageHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(messageHandlerFactory));
            }

            builder
                .RegisterInstance(messageHandlerFactory)
                .As<Func<HttpMessageHandler>>();
        }

        public static void RegisterEndpoints(this ContainerBuilder builder)
        {
            builder.RegisterEndpoint<RegistrationEndpoint>();
            builder.RegisterEndpoint<FlatContainerEndpoint>();
            builder.RegisterEndpoint<CatalogEndpoint>();
        }

        private static void RegisterEndpoint<T>(this ContainerBuilder builder)
            where T : class, IEndpoint
        {
            builder
                .RegisterType<T>()
                .AsSelf()
                .As<IEndpoint>();

            builder
                .RegisterType<EndpointValidator<T>>()
                .AsSelf()
                .As<IAggregateValidator>();
        }

        public static void RegisterValidators(this ContainerBuilder builder)
        {
            // Catalog validators
            builder.RegisterValidator<CatalogEndpoint, PackageHasSignatureValidator>();

            // Registration validators
            builder.RegisterValidator<RegistrationEndpoint, RegistrationExistsValidator>();
            builder.RegisterValidator<RegistrationEndpoint, RegistrationIdValidator>();
            builder.RegisterValidator<RegistrationEndpoint, RegistrationListedValidator>();
            builder.RegisterValidator<RegistrationEndpoint, RegistrationRequireLicenseAcceptanceValidator>();
            builder.RegisterValidator<RegistrationEndpoint, RegistrationVersionValidator>();

            // Flat-container validators
            builder.RegisterValidator<FlatContainerEndpoint, PackageIsRepositorySignedValidator>();
        }

        private static void RegisterValidator<TEndpoint, TValidator>(this ContainerBuilder builder)
            where TEndpoint : IEndpoint
            where TValidator : IValidator<TEndpoint>
        {
            builder
                .RegisterType<TValidator>()
                .As<IValidator<TEndpoint>>();
        }

        public static void RegisterSourceRepositories(
            this ContainerBuilder builder,
            string galleryUrl,
            string indexUrl)
        {
            if (string.IsNullOrEmpty(galleryUrl))
            {
                throw new ArgumentException(Strings.ArgumentMustNotBeNullOrEmpty, nameof(galleryUrl));
            }

            if (string.IsNullOrEmpty(indexUrl))
            {
                throw new ArgumentException(Strings.ArgumentMustNotBeNullOrEmpty, nameof(indexUrl));
            }

            builder
                .RegisterInstance(new PackageSource(galleryUrl))
                .Keyed<PackageSource>(FeedType.HttpV2);

            builder
                .RegisterInstance(new PackageSource(indexUrl))
                .Keyed<PackageSource>(FeedType.HttpV3);

            builder.RegisterDefaultResourceProviders();
            builder.RegisterV2ResourceProviders();
            builder.RegisterV3ResourceProviders();

            builder.RegisterSourceRepository(FeedType.HttpV2);
            builder.RegisterSourceRepository(FeedType.HttpV3);

            builder
                .RegisterType<ValidationSourceRepositories>()
                .WithParameter(
                    (pi, ctx) => pi.Name == "v2",
                    (pi, ctx) => ctx.ResolveKeyed<SourceRepository>(FeedType.HttpV2))
                .WithParameter(
                    (pi, ctx) => pi.Name == "v3",
                    (pi, ctx) => ctx.ResolveKeyed<SourceRepository>(FeedType.HttpV3))
                .AsSelf();
        }

        private static void RegisterSourceRepository(this ContainerBuilder builder, FeedType type)
        {
            builder
                .RegisterType<ValidationSourceRepository>()
                .WithParameter(
                    (pi, ctx) => pi.ParameterType == typeof(PackageSource),
                    (pi, ctx) => ctx.ResolveKeyed<PackageSource>(type))
                .WithParameter(
                    (pi, ctx) => pi.ParameterType == typeof(IEnumerable<INuGetResourceProvider>),
                    (pi, ctx) => ctx.ResolveKeyed<IEnumerable<INuGetResourceProvider>>(type))
                .WithParameter(TypedParameter.From(type))
                .Keyed<SourceRepository>(type);
        }

        private static void RegisterDefaultResourceProviders(this ContainerBuilder builder)
        {
            foreach (var provider in Repository.Provider.GetCoreV3())
            {
                builder
                    .RegisterInstance(provider)
                    .As<Lazy<INuGetResourceProvider>>();
            }
        }

        private static void RegisterV2ResourceProviders(this ContainerBuilder builder)
        {
            builder.RegisterResourceProvider<NonhijackableV2HttpHandlerResourceProvider>(FeedType.HttpV2);
            builder.RegisterResourceProvider<PackageTimestampMetadataResourceV2Provider>(FeedType.HttpV2);
            builder.RegisterResourceProvider<PackageRegistrationMetadataResourceV2FeedProvider>(FeedType.HttpV2);
        }

        private static void RegisterV3ResourceProviders(this ContainerBuilder builder)
        {
            builder.RegisterResourceProvider<PackageRegistrationMetadataResourceV3Provider>(FeedType.HttpV3);
        }

        private static void RegisterResourceProvider<TProvider>(this ContainerBuilder builder, FeedType type)
            where TProvider : INuGetResourceProvider
        {
            builder
                .RegisterType<TProvider>()
                .Keyed<INuGetResourceProvider>(type);
        }
    }
}

﻿using System;
using Fabric.Identity.API.Configuration;
using Fabric.Identity.API.ExtensionGrantValidators;
using Fabric.Identity.API.Persistence;
using Fabric.Identity.API.Persistence.CouchDb.DependencyInjection;
using Fabric.Identity.API.Persistence.InMemory.DependencyInjection;
using Fabric.Identity.API.Persistence.SqlServer.Configuration;
using Fabric.Identity.API.Persistence.SqlServer.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Fabric.Identity.API.Services
{
    public class IdentityServerInitializationService
    {
        private readonly IIdentityServerBuilder _identityServerBuilder;
        private readonly IServiceCollection _serviceCollection;
        private readonly IAppConfiguration _appConfiguration;
        private readonly HostingOptions _hostingOptions;
        private readonly IConnectionStrings _connectionStrings;
        private readonly ICertificateService _certificateService;
        private readonly ILogger _logger;

        public IdentityServerInitializationService(
            IIdentityServerBuilder identityServerBuilder,
            IServiceCollection serviceCollection,
            IAppConfiguration appConfiguration,
            HostingOptions hostingOptions,
            IConnectionStrings connectionStrings,
            ICertificateService certificateService,
            ILogger logger)
        {
            _identityServerBuilder = identityServerBuilder;
            _serviceCollection = serviceCollection;
            _appConfiguration = appConfiguration;
            _hostingOptions = hostingOptions;
            _connectionStrings = connectionStrings;
            _certificateService = certificateService;
            _logger = logger;
        }

        public void Initialize()
        {
            IIdentityServerConfigurator identityServerConfigurator;

            _identityServerBuilder.AddExtensionGrantValidator<DelegationGrantValidator>();
                
            if (string.Equals(FabricIdentityConstants.StorageProviders.CouchDb, _hostingOptions.StorageProvider,
                StringComparison.OrdinalIgnoreCase))
            {
                identityServerConfigurator =
                    new CouchDbIdentityServerConfigurator(_identityServerBuilder,
                        _serviceCollection,
                        _certificateService,
                        _appConfiguration.SigningCertificateSettings,
                        _appConfiguration.HostingOptions,
                        _appConfiguration.CouchDbSettings,
                        _logger);
            }
            else if (string.Equals(FabricIdentityConstants.StorageProviders.SqlServer, _hostingOptions.StorageProvider,
                StringComparison.OrdinalIgnoreCase))
            {
                identityServerConfigurator =
                    new SqlServerIdentityServerConfigurator(
                        _identityServerBuilder,
                        _serviceCollection,
                        _certificateService,
                        _appConfiguration.SigningCertificateSettings,
                        _appConfiguration.HostingOptions,
                        _connectionStrings,
                        _logger);
            }
            else if (string.Equals(FabricIdentityConstants.StorageProviders.InMemory, _hostingOptions.StorageProvider,
                StringComparison.OrdinalIgnoreCase))
            {
                identityServerConfigurator = new InMemoryIdentityServerConfigurator(_identityServerBuilder, _serviceCollection, _appConfiguration.HostingOptions, _logger);
            }
            else
            {
                throw new ArgumentException($"{_hostingOptions.StorageProvider} is an invalid storage provider type.");
            }

            identityServerConfigurator.Configure();
        }
    }
}

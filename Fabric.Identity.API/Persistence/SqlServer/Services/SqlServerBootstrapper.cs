﻿using System;
using System.Collections.Generic;
using Fabric.Identity.API.Persistence.SqlServer.Mappers;
using IdentityServer4.Models;
using Serilog;

namespace Fabric.Identity.API.Persistence.SqlServer.Services
{
    public class SqlServerBootstrapper : IDbBootstrapper
    {
        private readonly IIdentityDbContext _identityDbContext;
        private readonly ILogger _logger;

        public SqlServerBootstrapper(IIdentityDbContext identityDbContext, ILogger logger)
        {
            _identityDbContext = identityDbContext;
            _logger = logger;
        }

        public bool Setup()
        {
            // TODO: generate DB here
            return true;
        }

        public async void AddResources(IEnumerable<IdentityResource> resources)
        {
            foreach (var identityResource in resources)
            {
                try
                {
                    _identityDbContext.IdentityResources.Add(identityResource.ToEntity());
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, ex.Message);
                }
            }

            await _identityDbContext.SaveChangesAsync();
        }
    }
}
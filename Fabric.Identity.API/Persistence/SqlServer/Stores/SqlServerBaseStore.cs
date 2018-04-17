﻿using Fabric.Identity.API.Persistence.SqlServer.Services;
using Fabric.Identity.API.Services;
using IdentityServer4.Services;

namespace Fabric.Identity.API.Persistence.SqlServer.Stores
{
    public class SqlServerBaseStore
    {
        protected IIdentityDbContext IdentityDbContext;
        protected IUserResolverService UserResolverService;
        protected IEventService EventService;
        protected ISerializationSettings SerializationSettings;

        public SqlServerBaseStore(IIdentityDbContext identityDbContext,
            IEventService eventService,
            IUserResolverService userResolverService,
            ISerializationSettings serializationSettings)
        {
            IdentityDbContext = identityDbContext;
            EventService = eventService;
            UserResolverService = userResolverService;
            SerializationSettings = serializationSettings;
        }
    }
}

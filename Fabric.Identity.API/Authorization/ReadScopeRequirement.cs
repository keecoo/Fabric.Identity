﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;

namespace Fabric.Identity.API.Authorization
{
    public class ReadScopeRequirement : IAuthorizationRequirement, IHaveAuthorizationClaimType
    {
        public string ClaimType { get; } = FabricIdentityConstants.IdentityReadScope;
    }
}

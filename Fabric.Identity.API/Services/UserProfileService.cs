﻿using System.Linq;
using System.Threading.Tasks;
using Fabric.Identity.API.Extensions;
using Fabric.Identity.API.Persistence;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Serilog;

namespace Fabric.Identity.API.Services
{
    public class UserProfileService : IProfileService
    {
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;

        public UserProfileService(IUserStore userStore, ILogger logger)
        {
            _userStore = userStore;
            _logger = logger;
        }

        /// <summary>
        ///     This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo
        ///     endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.LogProfileRequest(_logger);

            if (context.RequestedClaimTypes.Any())
            {
                var user = await _userStore.FindBySubjectIdAsync(context.Subject.GetSubjectId());
                if (user != null)
                {
                    context.AddRequestedClaims(user.Claims);
                }
            }

            context.LogIssuedClaims(_logger);
        }

        /// <summary>
        ///     This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the
        ///     user's account has been deactivated since they logged in).
        ///     (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            _logger.Debug($"found sub from IsActiveContext: {sub}");
            var user = await _userStore.FindBySubjectIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}
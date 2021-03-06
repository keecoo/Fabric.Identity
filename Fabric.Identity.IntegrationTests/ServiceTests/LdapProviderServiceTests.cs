﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fabric.Identity.API.Configuration;
using Fabric.Identity.API.Infrastructure;
using Fabric.Identity.API.Services;
using Moq;
using Polly.CircuitBreaker;
using Serilog;
using Xunit;

namespace Fabric.Identity.IntegrationTests.ServiceTests
{
    public class LdapProviderServiceTests
    {
        [Fact]
        public async Task FindUser_Succeeds_WhenUserExists()
        {
            var logger = new Mock<ILogger>().Object;
            var settings = LdapTestHelper.GetLdapSettings();

            var ldapConnectionProvider = new LdapConnectionProvider(settings, logger);
            var newUser = LdapTestHelper.CreateTestUser("test", "user", settings.BaseDn, ldapConnectionProvider);
            var ldapProviderService = new LdapProviderService(ldapConnectionProvider, logger, new PolicyProvider());
            var externalUser = await ldapProviderService.FindUserBySubjectId($"EXAMPLE\\{newUser.getAttribute("cn").StringValue}");
            LdapTestHelper.RemoveEntry(newUser, ldapConnectionProvider);
            Assert.NotNull(externalUser);
            Assert.Equal("test", externalUser.FirstName);
            Assert.Equal("user", externalUser.LastName);
            Assert.Equal(@"EXAMPLE\test.user", externalUser.SubjectId);
            Assert.Equal("", externalUser.MiddleName);
        }

        [Fact]
        public async Task FindUser_ReturnsNull_WhenUserDoesNotExist()
        {
            var logger = new Mock<ILogger>().Object;
            var settings = LdapTestHelper.GetLdapSettings();

            var ldapConnectionProvider = new LdapConnectionProvider(settings, logger);
            var ldapProviderService = new LdapProviderService(ldapConnectionProvider, logger, new PolicyProvider());
            var externalUser = await ldapProviderService.FindUserBySubjectId($"EXAMPLE\\nonexistent.user");
            Assert.Null(externalUser);
        }

        [Fact]
        public async Task FindUser_ReturnsNull_WithNoConnectionInfo()
        {
            var logger = new Mock<ILogger>().Object;
            var settings = new LdapSettings
            {
                Server = "",
                Port = 389,
                Username = @"",
                Password = "",
                UseSsl = false
            };

            var ldapConnectionProvider = new LdapConnectionProvider(settings, logger);
            var ldapProviderService = new LdapProviderService(ldapConnectionProvider, logger, new PolicyProvider());
            var externalUser = await ldapProviderService.FindUserBySubjectId($"EXAMPLE\\nonexistent.user");
            Assert.Null(externalUser);
        }

        [Fact]
        public async Task FindUsers_ReturnsNull_WithBadConnection()
        {
            var ldapProviderService = GetLdapProviderSerivce(GetBadConnectionSettings(), new PolicyProvider());
            var externalUser = await ldapProviderService.FindUserBySubjectId("EXAMPLE\\nonexistent.user");
            Assert.Null(externalUser);
        }

        [Fact]
        public async Task FindUsers_TripsBreaker_WithBadConnection()
        {
            var policyProvider = new PolicyProvider();
            var ldapProviderService = GetLdapProviderSerivce(GetBadConnectionSettings(), policyProvider); 
            for (var i = 0; i < 6; i++)
            {
                var externalUser = await ldapProviderService.FindUserBySubjectId("EXAMPLE\\nonexistant.user");
                Assert.Null(externalUser);
            }
            Assert.Equal(CircuitState.Open, policyProvider.LdapErrorPolicy.CircuitState);
        }

        [Theory]
        [MemberData(nameof(SearchData))]
        public void SearchUsers_Succeeds(string searchText, int count)
        {
            var logger = new Mock<ILogger>().Object;
            var settings = LdapTestHelper.GetLdapSettings();

            var testUsers = new List<Tuple<string, string>>
            {
                Tuple.Create("mike", "trout"),
                Tuple.Create("mike", "piazza"),
                Tuple.Create("mike", "stanton"),
                Tuple.Create("carlos", "beltran")
            };

            var ldapConnectionProvider = new LdapConnectionProvider(settings, logger);
            var ldapEntries = LdapTestHelper.CreateTestUsers(testUsers, settings.BaseDn, ldapConnectionProvider);

            var ldapProviderService = new LdapProviderService(ldapConnectionProvider, logger, new PolicyProvider());
            var searchResults = ldapProviderService.SearchUsers(searchText);
            LdapTestHelper.RemoveEntries(ldapEntries, ldapConnectionProvider);

            Assert.NotNull(searchResults);
            Assert.Equal(count, searchResults.Count);

        }

        public static IEnumerable<object[]> SearchData => new[]
        {
            new object[] {"mike", 3},
            new object[] {"mike.piazza", 1},
            new object[] {"car", 1},
            new object[] {"belt", 1},
            new object[] {"griffey", 0},
        };

        private LdapSettings GetBadConnectionSettings()
        {
            return new LdapSettings
            {
                Server = "nonexistent",
                Port = 123,
                Username = "test",
                Password = "password",
                UseSsl = false
            };
        }

        private LdapProviderService GetLdapProviderSerivce(LdapSettings settings, PolicyProvider policyProvider)
        {
            var logger = new Mock<ILogger>().Object;
            var ldapConnectionProvider = new LdapConnectionProvider(settings, logger);
            return new LdapProviderService(ldapConnectionProvider, logger, policyProvider);
        }
    }
}

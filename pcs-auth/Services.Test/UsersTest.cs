// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Azure.IoTSolutions.Auth.Services;
using Microsoft.Azure.IoTSolutions.Auth.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class UsersTest
    {
        private readonly Mock<ILogger> logger;
        private readonly Mock<IServicesConfig> servicesConfig;
        private readonly Mock<IPolicies> policiesMock;
        private readonly IUsers users;
        private const string ADMIN_ROLE_KEY = "Admin";
        private const string OPERATOR_ROLE_KEY = "Operator";
        private const string READONLY_ROLE_KEY = "ReadOnly";
        private const string ID_KEY = "oid";
        private const string NAME_KEY = "name";
        private const string EMAIL_KEY = "email";

        public UsersTest()
        {
            this.logger = new Mock<ILogger>();
            this.servicesConfig = new Mock<IServicesConfig>();
            this.servicesConfig.SetupProperty(x => x.JwtRolesFrom, "roles");
            this.servicesConfig.SetupProperty(x => x.JwtEmailFrom, new List<string>(){"email"});
            this.servicesConfig.SetupProperty(x => x.JwtNameFrom, new List<string>() { "name" });
            this.servicesConfig.SetupProperty(x => x.JwtUserIdFrom, new List<string>() { "oid" });
            this.policiesMock = new Mock<IPolicies>();
           
            this.users = new Users(
                this.servicesConfig.Object,
                this.logger.Object,
                this.policiesMock.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetUserInfoWithClaim_ReturnsValues()
        {
            // Arrange
            List<Claim> claims = this.GetClaimWithUserInfo();
            var adminPolicy = this.GetAdminPolicy();
            var readOnlyPolicy = this.GetReadOnlyPolicy();
            this.policiesMock.Setup(x => x.GetByRole(ADMIN_ROLE_KEY)).Returns(adminPolicy);
            this.policiesMock.Setup(x => x.GetByRole(READONLY_ROLE_KEY)).Returns(readOnlyPolicy);

            // Act
            var result = this.users.GetUserInfo(claims);

            // Assert
            Assert.Equal(claims.FirstOrDefault(k => k.Type == EMAIL_KEY).Value, result.Email);
            Assert.Equal(claims.FirstOrDefault(k => k.Type == NAME_KEY).Value, result.Name);
            Assert.Equal(claims.FirstOrDefault(k => k.Type == ID_KEY).Value, result.Id);
            Assert.NotEmpty(result.AllowedActions);
            foreach (var action in adminPolicy.AllowedActions)
            {
                Assert.Contains(action, result.AllowedActions);
            }
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetAllowedActionsForRoles_ReturnsValues()
        {
            // Arrange
            var adminPolicy = this.GetAdminPolicy();
            var operatorPolicy = this.GetOperatorPolicy();
            var readOnlyPolicy = this.GetReadOnlyPolicy();
            this.policiesMock.Setup(x => x.GetByRole(ADMIN_ROLE_KEY)).Returns(adminPolicy);
            this.policiesMock.Setup(x => x.GetByRole(OPERATOR_ROLE_KEY)).Returns(operatorPolicy);
            this.policiesMock.Setup(x => x.GetByRole(READONLY_ROLE_KEY)).Returns(readOnlyPolicy);

            // Act
            var adminActions = this.users.GetAllowedActions(new List<string>{ ADMIN_ROLE_KEY, OPERATOR_ROLE_KEY });
            var operatorActions = this.users.GetAllowedActions(new List<string> { OPERATOR_ROLE_KEY, READONLY_ROLE_KEY });
            var readonlyActions = this.users.GetAllowedActions(new List<string> { READONLY_ROLE_KEY });

            // Assert
            Assert.Equal(adminPolicy.AllowedActions, adminActions);
            Assert.Equal(operatorPolicy.AllowedActions, operatorActions);
            Assert.Empty(readonlyActions);
        }

        private List<Claim> GetClaimWithUserInfo()
        {
            return new List<Claim>()
            {
                new Claim("aud", "1239a0f - 1230 - 123d - 1235 - 123e88123df7"),g
                new Claim("iss", "https://sts.windows.net/123123bf-1231-123f-123b-123123123123/"),
                new Claim("iat", "1231941234"),
                new Claim("nbf", "1231941234"),
                new Claim("exp", "1231941234"),
                new Claim("aio", "AWQAm/8IAAAA2b4As+QT8/IN4123noz1FOSeh9kviI123MQdpkg1231YKIzB41238syc5xi123p5h123qhubot123GeggQBViT123SeK123iYk0skQ123xO05L7123g9123KE1azzwhPH"),
                new Claim("amr", "rsa"),
                new Claim("amr", "mfa"),
                new Claim("family_name", "LastNameTest"),
                new Claim("given_name", "FirstNameTest"),
                new Claim(NAME_KEY, "Test Name"),
                new Claim("ipaddr", "131.101.164.180"),
                new Claim("nonce", "123"),
                new Claim(ID_KEY, "12341234-1234-1234-1234-123412341234"),
                new Claim("onprem_sid", "123"),
                new Claim("roles", ADMIN_ROLE_KEY),
                new Claim("roles", READONLY_ROLE_KEY),
                new Claim("sub", "123"),
                new Claim("tid", "123"),
                new Claim("unique_name", "test123@microsoft.com"),
                new Claim(EMAIL_KEY, "test123@microsoft.com"),
                new Claim("upn", "test123@microsoft.com"),
                new Claim("uti", "123"),
                new Claim("var", "1.0")
            };
        }

        private Policy GetAdminPolicy()
        {
            var allowedActions = new List<string>()
            {
                "UpdateAlarms",
                "DeleteAlarms",
                "CreateDevices",
                "UpdateDevices",
                "DeleteDevices",
                "CreateDeviceGroups",
                "UpdateDeviceGroups",
                "DeleteDeviceGroups",
                "CreateRules",
                "UpdateRules",
                "DeleteRules",
                "CreateJobs",
                "UpdateSimManagement"
            };

            return new Policy()
            {
                AllowedActions = allowedActions,
                Id = "a400a00b-f67c-42b7-ba9a-f73d8c67e433",
                Role = ADMIN_ROLE_KEY
            };
        }

        private Policy GetOperatorPolicy()
        {
            var allowedActions = new List<string>()
            {
                "UpdateAlarms",
                "CreateDevices",
                "UpdateDevices",
                "CreateDeviceGroups",
                "UpdateDeviceGroups",
                "CreateRules",
                "UpdateRules",
                "CreateJobs",
                "UpdateSimManagement"
            };

            return new Policy()
            {
                AllowedActions = allowedActions,
                Id = "d607a063-f67c-42b7-ba9a-f73d8c67e433",
                Role = OPERATOR_ROLE_KEY
            };
        }

        private Policy GetReadOnlyPolicy()
        {
            var allowedActions = new List<string>();

            return new Policy()
            {
                AllowedActions = allowedActions,
                Id = "e5bbd0f5-128e-4362-9dd1-8f253c6082d7",
                Role = READONLY_ROLE_KEY
            };
        }
    }
}

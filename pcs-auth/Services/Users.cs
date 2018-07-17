﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Azure.IoTSolutions.Auth.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.Auth.Services
{
    public interface IUsers
    {
        User GetUserInfo(IEnumerable<Claim> claims);
    }

    public class Users : IUsers
    {
        private readonly ILogger log;
        private readonly IServicesConfig config;
        private readonly IPolicies policies;
        private readonly string rolesKey;

        public Users(
            IServicesConfig config,
            ILogger log,
            IPolicies policies)
        {
            this.config = config;
            this.log = log;
            this.policies = policies;
            this.rolesKey = this.config.JwtRolesFrom.ToLowerInvariant();
        }

        public User GetUserInfo(IEnumerable<Claim> claims)
        {
            // Map all the claims into a dictionary
            var data = new Dictionary<string,string>();
            var roles = new List<string>();

            foreach (var c in claims)
            {
                data[c.Type.ToLowerInvariant()] = c.Value;

                // There can be multiple roles, add all roles to an array
                if (string.Equals(c.Type.ToLowerInvariant(), this.rolesKey, StringComparison.OrdinalIgnoreCase))
                {
                    roles.Add(c.Value);
                }
            }

            // Extract user information from the claims
            var id = this.config.JwtUserIdFrom
                .Select(key => key.ToLowerInvariant())
                .Where(k => data.ContainsKey(k))
                .Aggregate("", (current, k) => current + ((string)data[k] + ' '))
                .TrimEnd();
            var name = this.config.JwtNameFrom
                .Select(key => key.ToLowerInvariant())
                .Where(k => data.ContainsKey(k))
                .Aggregate("", (current, k) => current + ((string)data[k] + ' '))
                .TrimEnd();
            var email = this.config.JwtEmailFrom
                .Select(key => key.ToLowerInvariant())
                .Where(k => data.ContainsKey(k))
                .Aggregate("", (current, k) => current + ((string)data[k] + ' '))
                .TrimEnd();

            // Get allowed actions based on policy
            var allowedActions = this.GetAllowedActions(roles);

            if (string.IsNullOrEmpty(id)) id = "-unknown-";
            if (string.IsNullOrEmpty(name)) name = "user name unknown";
            if (string.IsNullOrEmpty(email)) email = "email address unknown";         

            return new User
            {
                Id = id,
                Name = name,
                Email = email,
                AllowedActions = allowedActions
            };
        }

        private List<string> GetAllowedActions(List<string> roles)
        {
            List<string> allowedActions = new List<string>();
            foreach (var role in roles)
            {
                var policy = this.policies.GetByRole(role);
                allowedActions.AddRange(policy.AllowedActions);
            }

            return allowedActions;
        }
    }
}

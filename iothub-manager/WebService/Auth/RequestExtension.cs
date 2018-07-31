// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Auth
{
    public static class RequestExtension
    {
        private const string CONTEXT_KEY_USER_CLAIMS = "CurrentUserClaims";
        private const string CONTEXT_KEY_AUTH_REQUIRED = "AuthRequired";
        private const string CONTEXT_KEY_ALLOWED_ACTIONS = "CurrentUserAllowedActions";
        // Role claim type
        private const string ROLE_CLAIM_TYPE = "roles";
        private const string USER_OBJECT_ID_CLAIM_TYPE = "oid";

        // Store the current user claims in the current request
        public static void SetCurrentUserClaims(this HttpRequest request, IEnumerable<Claim> claims)
        {
            request.HttpContext.Items[CONTEXT_KEY_USER_CLAIMS] = claims;
        }

        // Get the user claims from the current request
        public static IEnumerable<Claim> GetCurrentUserClaims(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(CONTEXT_KEY_USER_CLAIMS))
            {
                return new List<Claim>();
            }

            return request.HttpContext.Items[CONTEXT_KEY_USER_CLAIMS] as IEnumerable<Claim>;
        }


        // Store authentication setting in the current request
        public static void SetAuthRequired(this HttpRequest request, bool authRequired)
        {
            request.HttpContext.Items[CONTEXT_KEY_AUTH_REQUIRED] = authRequired;
        }

        // Get the authentication setting in the current request
        public static bool GetAuthRequired(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(CONTEXT_KEY_AUTH_REQUIRED))
            {
                return true;
            }

            return (bool)request.HttpContext.Items[CONTEXT_KEY_AUTH_REQUIRED];
        }

        // Get the user's role claims from the current request
        public static string GetCurrentUserObjectId(this HttpRequest request)
        {
            var claims = GetCurrentUserClaims(request);
            return claims.Where(c => c.Type.ToLowerInvariant().Equals(USER_OBJECT_ID_CLAIM_TYPE, StringComparison.CurrentCultureIgnoreCase))
                .Select(c => c.Value).First();
        }

        // Get the user's role claims from the current request
        public static IEnumerable<string> GetCurrentUserRoleClaim(this HttpRequest request)
        {
            var claims = GetCurrentUserClaims(request);
            return claims.Where(c => c.Type.ToLowerInvariant().Equals(ROLE_CLAIM_TYPE, StringComparison.CurrentCultureIgnoreCase))
                .Select(c => c.Value);
        }

        // Store the current user allowed actions in the current request
        public static void SetCurrentUserAllowedActions(this HttpRequest request, IEnumerable<string> allowedActions)
        {
            request.HttpContext.Items[CONTEXT_KEY_ALLOWED_ACTIONS] = allowedActions;
        }

        // Get the user's allowed actions from the current request
        public static IEnumerable<string> GetCurrentUserAllowedActions(this HttpRequest request)
        {
            if (!request.HttpContext.Items.ContainsKey(CONTEXT_KEY_ALLOWED_ACTIONS))
            {
                return new List<string>();
            }

            return request.HttpContext.Items[CONTEXT_KEY_ALLOWED_ACTIONS] as IEnumerable<string>;
        }
    }
}

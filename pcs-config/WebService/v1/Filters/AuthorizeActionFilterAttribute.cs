// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.Auth;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters
{
    public class AuthorizeAttribute : TypeFilterAttribute
    {
        public AuthorizeAttribute(string allowedActions)
            : base(typeof(AuthorizeActionFilterAttribute))
        {
            this.Arguments = new object[] { allowedActions };
        }
    }

    public class AuthorizeActionFilterAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string allowedAction;

        public AuthorizeActionFilterAttribute(string allowedAction)
        {
            this.allowedAction = allowedAction;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool isAuthorized = this.ValidateAuthorization(context.HttpContext, this.allowedAction);

            if (!isAuthorized)
            {
                throw new NoAuthorizationException($"Current user is not authorized to perform this action: '{this.allowedAction}'");
            }
            else
            {
                await next();
            }
        }

        /// <summary>
        /// Validate allowed actions of current user based on role claims against the declared actions
        /// in the Authorize attribute of controller. The allowed action is case insensitive.
        /// </summary>
        /// <param name="httpContext">current context of http request</param>
        /// <param name="allowedAction">allowed action required by controller</param>
        /// <returns>true if validatation succeed</returns>
        private bool ValidateAuthorization(HttpContext httpContext, string allowedAction)
        {
            if (!httpContext.Request.GetAuthRequired() || !httpContext.Request.IsExternalRequest()) return true;

            if (allowedAction == null || !allowedAction.Any()) return true;

            var userAllowedActions = httpContext.Request.GetCurrentUserAllowedActions();
            if (userAllowedActions == null || !userAllowedActions.Any())
            {
                return false;
            }

            // validation succeeds if any required action occurs in the current user's allowed allowedAction
            return userAllowedActions.Select(a => a.ToLowerInvariant())
               .Contains(this.allowedAction.ToLowerInvariant());
        }
    }
}

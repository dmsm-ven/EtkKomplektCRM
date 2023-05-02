using EtkBlazorApp.Model.IOptionProfiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace EtkBlazorApp.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class Integration1CAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var options = context.HttpContext.RequestServices.GetService<IOptions<Integration1C_Configuration>>().Value;

            if (!context.HttpContext.Request.Headers.TryGetValue("Authorize", out var auth))
            {
                context.Result = new BadRequestResult();
                return;
            }

            string senderToken = auth.ToString()
                .Replace("Token", string.Empty)
                .Trim();

            bool isAuthorized = senderToken.Equals(options.Token, System.StringComparison.OrdinalIgnoreCase);

            if (!isAuthorized)
            {
                context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
                return;
            }
        }
    }
}
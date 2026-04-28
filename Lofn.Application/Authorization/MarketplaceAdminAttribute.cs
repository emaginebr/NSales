using Lofn.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NAuth.ACL.Interfaces;
using System;
using System.Threading.Tasks;

namespace Lofn.Application.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class MarketplaceAdminAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var services = context.HttpContext.RequestServices;
            var tenantResolver = services.GetRequiredService<ITenantResolver>();
            var userClient = services.GetRequiredService<IUserClient>();

            var userSession = userClient.GetUserInSession(context.HttpContext);
            if (userSession == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!userSession.IsAdmin)
            {
                context.Result = new ForbidResult();
                return;
            }

            if (!tenantResolver.Marketplace)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}

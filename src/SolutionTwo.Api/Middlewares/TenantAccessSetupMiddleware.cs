using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Common.MultiTenancy;

namespace SolutionTwo.Api.Middlewares;

public class TenantAccessSetupMiddleware
{
    private readonly RequestDelegate _next;

    public TenantAccessSetupMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantAccessSetter tenantAccessSetter)
    {
        if (UnauthorizedAccessAllowed(context) || context.User.IsInRole(UserRoles.SuperAdmin))
        {
            tenantAccessSetter.SetAccessToAllTenants();
        }
        else
        {
            var claimsValue = context.User.Claims.FirstOrDefault(x => x.Type == MultiTenancyClaimNames.TenantId)?.Value;
            if (!string.IsNullOrEmpty(claimsValue) && Guid.TryParse(claimsValue, out var tenantId))
            {
                tenantAccessSetter.SetAccessToTenant(tenantId);
            }
            else
            {
                throw new ApplicationException("TenantAccessSetter can't set TenantId");
            }
        }

        await _next(context);
    }
    
    private static bool UnauthorizedAccessAllowed(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var authorizeAttribute = endpoint?.Metadata.GetMetadata<SolutionTwoAuthorizeAttribute>();
        var allowAnonymousAttribute = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>();

        return authorizeAttribute == null || allowAnonymousAttribute != null;
    }
}
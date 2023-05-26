using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using SolutionTwo.Api.Attributes;

namespace SolutionTwo.Api.Middlewares;

public class RoleBasedAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    
    private const int BadResultStatusCode = (int)HttpStatusCode.Forbidden;
    
    public RoleBasedAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
 
    public async Task InvokeAsync(HttpContext context)
    {
        if (UnauthorizedAccessAllowed(context, out var roles) || roles == null || roles.Length == 0)
        {
            await _next(context);
            return;
        }
        
        var authorized = roles.Any(x => context.User.IsInRole(x));
        if (!authorized)
        {
            context.Response.StatusCode = BadResultStatusCode;
            return;
        }

        await _next(context);
    }

    private static bool UnauthorizedAccessAllowed(HttpContext context, out string[]? roles)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var authorizeAttribute = endpoint?.Metadata.GetMetadata<RoleBasedAuthorizeAttribute>();
        var allowAnonymousAttribute = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>();

        roles = authorizeAttribute?.Roles;
        
        return authorizeAttribute == null || allowAnonymousAttribute != null;
    }
}
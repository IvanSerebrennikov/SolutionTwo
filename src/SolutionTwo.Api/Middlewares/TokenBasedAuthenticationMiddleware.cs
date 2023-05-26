using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Identity.TokenManagement.Interfaces;

namespace SolutionTwo.Api.Middlewares;

public class TokenBasedAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenManager _tokenManager;
    
    private const string AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
    private const int BadResultStatusCode = (int)HttpStatusCode.Unauthorized;
 
    public TokenBasedAuthenticationMiddleware(RequestDelegate next, ITokenManager tokenManager)
    {
        _next = next;
        _tokenManager = tokenManager;
    }
 
    public async Task InvokeAsync(HttpContext context)
    {
        if (UnauthorizedAccessAllowed(context))
        {
            await _next(context);
            return;
        }
        
        string authHeader = context.Request.Headers.Authorization;
        var authSchemeWithSpace = $"{AuthenticationScheme} ";

        if (authHeader == null || !authHeader.StartsWith(authSchemeWithSpace))
        {
            context.Response.StatusCode = BadResultStatusCode;
            return;
        }
        
        var tokenString = authHeader.Substring(authSchemeWithSpace.Length).Trim();

        if (string.IsNullOrEmpty(tokenString))
        {
            context.Response.StatusCode = BadResultStatusCode;
            return;
        }

        var claimsPrincipal = _tokenManager.ValidateTokenAndGetPrincipal(tokenString, out var securityToken);
        
        if (claimsPrincipal == null || 
            securityToken == null || 
            !Guid.TryParse(securityToken.Id, out var authTokenId) || 
            _tokenManager.IsTokenDeactivated(authTokenId))
        {
            context.Response.StatusCode = BadResultStatusCode;
            return;
        }
        
        context.User = claimsPrincipal;
 
        await _next(context);
    }

    private static bool UnauthorizedAccessAllowed(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var authorizeAttribute = endpoint?.Metadata.GetMetadata<RoleBasedAuthorizeAttribute>();
        var allowAnonymousAttribute = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>();

        return authorizeAttribute == null || allowAnonymousAttribute != null;
    }
}
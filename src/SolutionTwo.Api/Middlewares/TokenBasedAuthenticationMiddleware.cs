using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Business.Identity.TokenProvider.Interfaces;

namespace SolutionTwo.Api.Middlewares;

public class TokenBasedAuthenticationMiddleware
{
    private const string AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
    private const int BadResultStatusCode = (int)HttpStatusCode.Unauthorized;
    private readonly RequestDelegate _next;
    private readonly ITokenProvider _tokenProvider;

    public TokenBasedAuthenticationMiddleware(RequestDelegate next, ITokenProvider tokenProvider)
    {
        _next = next;
        _tokenProvider = tokenProvider;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
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

        var claimsPrincipal = _tokenProvider.ValidateAuthTokenAndGetPrincipal(tokenString, out var securityToken);

        if (claimsPrincipal == null ||
            securityToken == null ||
            !Guid.TryParse(securityToken.Id, out var authTokenId) ||
            authService.IsAuthTokenRevoked(authTokenId))
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
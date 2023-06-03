using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Common.MultiTenancy;

namespace SolutionTwo.Api.Middlewares;

public class TokenBasedAuthenticationMiddleware
{
    private const string AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
    private const int BadResultStatusCode = (int)HttpStatusCode.Unauthorized;
    private readonly RequestDelegate _next;
    private readonly HardCodedIdentityConfiguration _hardCodedIdentityConfiguration;
    private readonly IWebHostEnvironment _env;

    public TokenBasedAuthenticationMiddleware(
        RequestDelegate next, 
        HardCodedIdentityConfiguration hardCodedIdentityConfiguration, 
        IWebHostEnvironment env)
    {
        _next = next;
        _hardCodedIdentityConfiguration = hardCodedIdentityConfiguration;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, IIdentityService identityService)
    {
        if (UnauthorizedAccessAllowed(context))
        {
            await _next(context);
            return;
        }

        if (_env.IsDevelopment() && _hardCodedIdentityConfiguration.UseHardCodedIdentity == true)
        {
            ConfigureHardCodedIdentity(context);
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

        var verificationResult = identityService.VerifyAuthTokenAndGetPrincipal(tokenString);

        if (!verificationResult.IsSucceeded || verificationResult.Data == null)
        {
            context.Response.StatusCode = BadResultStatusCode;
            return;
        }

        context.User = verificationResult.Data;

        await _next(context);
    }

    private static bool UnauthorizedAccessAllowed(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var authorizeAttribute = endpoint?.Metadata.GetMetadata<RoleBasedAuthorizeAttribute>();
        var allowAnonymousAttribute = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>();

        return authorizeAttribute == null || allowAnonymousAttribute != null;
    }

    private void ConfigureHardCodedIdentity(HttpContext context)
    {
        var claims = _hardCodedIdentityConfiguration.Roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
        claims.Add(new Claim(ClaimTypes.Name, _hardCodedIdentityConfiguration.Username!));
        claims.Add(new Claim(MultiTenancyClaimNames.TenantId,
            _hardCodedIdentityConfiguration.TenantId!.Value.ToString()));
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
    }
}
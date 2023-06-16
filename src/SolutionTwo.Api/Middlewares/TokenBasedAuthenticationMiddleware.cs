using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.Services.Interfaces;
using SolutionTwo.Common.Constants;
using SolutionTwo.Common.LoggedInUserAccessor.Interfaces;

namespace SolutionTwo.Api.Middlewares;

public class TokenBasedAuthenticationMiddleware
{
    private const int UnauthorizedStatusCode = (int)HttpStatusCode.Unauthorized;
    private const string AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
    
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

    public async Task InvokeAsync(
        HttpContext context, 
        IIdentityService identityService, 
        ILoggedInUserSetter loggedInUserSetter)
    {
        if (UnauthorizedAccessAllowed(context))
        {
            await _next(context);
            return;
        }

        ClaimsPrincipal? user;
        if (!_env.IsProduction() && _hardCodedIdentityConfiguration.UseHardCodedIdentity == true)
        {
            user = GetHardCodedIdentity();
        }
        else
        {
            user = GetRealUserIdentity(context, identityService);
        }

        if (user == null)
        {
            context.Response.StatusCode = UnauthorizedStatusCode;
            return;
        }
        
        context.User = user;

        SetLoggedInUserId(user, loggedInUserSetter);

        await _next(context);
    }

    private static bool UnauthorizedAccessAllowed(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var authorizeAttribute = endpoint?.Metadata.GetMetadata<SolutionTwoAuthorizeAttribute>();
        var allowAnonymousAttribute = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>();

        return authorizeAttribute == null || allowAnonymousAttribute != null;
    }

    private ClaimsPrincipal GetHardCodedIdentity()
    {
        var claims = _hardCodedIdentityConfiguration.Roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
        claims.Add(new Claim(ClaimTypes.Name, _hardCodedIdentityConfiguration.Username!));
        claims.Add(new Claim(ClaimTypes.NameIdentifier, _hardCodedIdentityConfiguration.UserId!.Value.ToString()));
        claims.Add(new Claim(SolutionTwoClaimNames.TenantId,
            _hardCodedIdentityConfiguration.TenantId!.Value.ToString()));
        
        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }
    
    private ClaimsPrincipal? GetRealUserIdentity(HttpContext context, IIdentityService identityService)
    {
        string authHeader = context.Request.Headers.Authorization;
        var authSchemeWithSpace = $"{AuthenticationScheme} ";

        if (authHeader == null || !authHeader.StartsWith(authSchemeWithSpace))
        {
            return null;
        }

        var tokenString = authHeader.Substring(authSchemeWithSpace.Length).Trim();

        if (string.IsNullOrEmpty(tokenString))
        {
            return null;
        }

        var verificationResult = identityService.VerifyAuthTokenAndGetPrincipal(tokenString);

        if (!verificationResult.IsSucceeded || verificationResult.Data == null)
        {
            return null;
        }
        
        return verificationResult.Data;
    }

    private void SetLoggedInUserId(ClaimsPrincipal user, ILoggedInUserSetter loggedInUserSetter)
    {
        var claimsValue = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(claimsValue) && Guid.TryParse(claimsValue, out var userId))
        {
            loggedInUserSetter.SetLoggedInUserId(userId);
        }
        else
        {
            throw new ApplicationException($"{nameof(ILoggedInUserSetter)} can't set UserId");
        }
    }
}
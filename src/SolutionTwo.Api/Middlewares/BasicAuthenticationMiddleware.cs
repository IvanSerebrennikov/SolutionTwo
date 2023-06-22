﻿using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Api.Configuration;

namespace SolutionTwo.Api.Middlewares;

public class BasicAuthenticationMiddleware
{
    private const string AuthenticationHeaderKey = "BasicAuthentication";
    private const string AuthenticationScheme = "basic";
    
    private readonly RequestDelegate _next;
    private readonly BasicAuthenticationConfiguration _basicAuthenticationConfiguration;

    public BasicAuthenticationMiddleware(
        RequestDelegate next,
        BasicAuthenticationConfiguration basicAuthenticationConfiguration)
    {
        _next = next;
        _basicAuthenticationConfiguration = basicAuthenticationConfiguration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (UnauthorizedAccessAllowed(context))
        {
            await _next(context);
            return;
        }

        var authenticated = IsAuthenticated(context);
        if (!authenticated)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next(context);
    }

    private static bool UnauthorizedAccessAllowed(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var authAttribute = endpoint?.Metadata.GetMetadata<BasicAuthenticationAttribute>();
        var allowAnonymousAttribute = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>();
        
        return authAttribute == null || allowAnonymousAttribute != null;
    }

    private bool IsAuthenticated(HttpContext context)
    {
        var containsKey = context.Request.Headers.ContainsKey(AuthenticationHeaderKey);
        if (!containsKey)
            return false;
        
        var authHeader = context.Request.Headers.FirstOrDefault(x => x.Key == AuthenticationHeaderKey).Value.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith(AuthenticationScheme))
            return false;
        
        var authSchemeWithSpace = $"{AuthenticationScheme} ";
        var credentialsString = authHeader.Substring(authSchemeWithSpace.Length).Trim();

        if (string.IsNullOrEmpty(credentialsString))
            return false;

        var credentials = Encoding.ASCII.GetString(Convert.FromBase64String(credentialsString)).Split(':');

        return credentials.Length == 2 && 
               credentials[0] == _basicAuthenticationConfiguration.Username &&
               credentials[1] == _basicAuthenticationConfiguration.Password;
    }
}
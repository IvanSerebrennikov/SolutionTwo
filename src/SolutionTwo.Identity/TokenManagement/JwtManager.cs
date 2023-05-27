﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Identity.Configuration;
using SolutionTwo.Identity.TokenManagement.Interfaces;

namespace SolutionTwo.Identity.TokenManagement;

public class JwtManager : ITokenManager
{
    private readonly JwtConfiguration _jwtConfiguration;
    private readonly IMemoryCache _memoryCache;

    public JwtManager(JwtConfiguration jwtConfiguration, IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _jwtConfiguration = jwtConfiguration;
    }

    public string GenerateAuthToken(List<(string, string)> claims, out Guid authTokenId)
    {
        var claimsList = claims.Select(x => new Claim(x.Item1, x.Item2)).ToList();

        authTokenId = Guid.NewGuid();
        claimsList.Add(new Claim(JwtRegisteredClaimNames.Jti, authTokenId.ToString()));

        var key = GetSymmetricSecurityKey();
        var tokenHandler = new JwtSecurityTokenHandler();

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            issuer: _jwtConfiguration.JwtIssuer,
            audience: _jwtConfiguration.JwtAudience,
            claims: claimsList,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfiguration.JwtExpiresMinutes!.Value),
            notBefore: DateTime.UtcNow,
            signingCredentials: credentials);
        
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
    
    public ClaimsPrincipal? ValidateAuthTokenAndGetPrincipal(string tokenString, out SecurityToken? securityToken)
    {
        var key = GetSymmetricSecurityKey();
        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            var principal = tokenHandler.ValidateToken(tokenString, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtConfiguration.JwtIssuer,
                ValidAudience = _jwtConfiguration.JwtAudience,
                IssuerSigningKey = key
            }, out var validatedToken);

            securityToken = validatedToken;
            return validatedToken != null ? principal : null;
        }
        catch
        {
            securityToken = null;
            return null;
        }
    }

    public bool IsAuthTokenRevoked(Guid authTokenId)
    {
        return _memoryCache.TryGetValue(GetDeactivatedTokenKey(authTokenId), out int _);
    }

    public void RevokeAuthToken(Guid authTokenId)
    {
        _memoryCache.Set(GetDeactivatedTokenKey(authTokenId), 1,
            TimeSpan.FromMinutes(_jwtConfiguration.JwtExpiresMinutes!.Value));
    }
    
    private static string GetDeactivatedTokenKey(Guid authTokenId)
    {
        return $"auth-tokens:{authTokenId}:revoked";
    }

    private SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
            _jwtConfiguration.JwtKey!));
    }
}
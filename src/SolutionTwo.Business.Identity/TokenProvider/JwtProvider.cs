using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Business.Identity.TokenProvider.Interfaces;

namespace SolutionTwo.Business.Identity.TokenProvider;

public class JwtProvider : ITokenProvider
{
    private readonly IdentityConfiguration _identityConfiguration;

    public JwtProvider(IdentityConfiguration identityConfiguration)
    {
        _identityConfiguration = identityConfiguration;
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
            issuer: _identityConfiguration.JwtIssuer,
            audience: _identityConfiguration.JwtAudience,
            claims: claimsList,
            expires: DateTime.UtcNow.AddMinutes(_identityConfiguration.JwtExpiresMinutes!.Value),
            notBefore: DateTime.UtcNow,
            signingCredentials: credentials);
        
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
    
    public ClaimsPrincipal? ValidateAuthToken(string tokenString, out SecurityToken? securityToken)
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
                ValidIssuer = _identityConfiguration.JwtIssuer,
                ValidAudience = _identityConfiguration.JwtAudience,
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

    private SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
            _identityConfiguration.JwtKey!));
    }
}
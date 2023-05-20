using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Identity.Configuration;
using SolutionTwo.Identity.TokenProvider.Interfaces;

namespace SolutionTwo.Identity.TokenProvider;

public class JwtProvider : ITokenProvider
{
    private readonly IdentityConfiguration _identityConfiguration;

    public JwtProvider(IConfiguration configuration)
    {
        _identityConfiguration = configuration.GetSection<IdentityConfiguration>();
    }

    public string GenerateAuthToken(params (string, string)[] claims)
    {
        var claimsList = claims.Select(x => new Claim(x.Item1, x.Item2)).ToList();

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
            _identityConfiguration.JwtKey!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            issuer: _identityConfiguration.JwtIssuer,
            audience: _identityConfiguration.JwtAudience,
            claims: claimsList,
            expires: DateTime.UtcNow.AddMinutes(_identityConfiguration.JwtExpiresMinutes!.Value),
            notBefore: DateTime.UtcNow,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
}
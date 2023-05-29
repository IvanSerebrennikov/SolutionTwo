using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace SolutionTwo.Business.Identity.TokenProvider.Interfaces;

public interface ITokenProvider
{
    string GenerateAuthToken(List<(string, string)> claims, out Guid authTokenId);

    ClaimsPrincipal? ValidateAuthTokenAndGetPrincipal(string tokenString, out SecurityToken? securityToken);
}
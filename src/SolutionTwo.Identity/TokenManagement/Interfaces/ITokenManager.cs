using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace SolutionTwo.Identity.TokenManagement.Interfaces;

public interface ITokenManager
{
    string GenerateAuthToken(List<(string, string)> claims, out Guid authTokenId);

    ClaimsPrincipal? ValidateTokenAndGetPrincipal(string tokenString, out SecurityToken? securityToken);
    
    bool IsTokenDeactivated(Guid authTokenId);
    
    void DeactivateToken(Guid authTokenId);
}
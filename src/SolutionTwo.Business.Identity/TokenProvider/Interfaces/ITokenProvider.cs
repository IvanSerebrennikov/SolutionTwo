using System.Security.Claims;

namespace SolutionTwo.Business.Identity.TokenProvider.Interfaces;

public interface ITokenProvider
{
    string GenerateAuthToken(List<(string, string)> claims, out Guid authTokenId);

    ClaimsPrincipal? ValidateAuthToken(string tokenString, out Guid? authTokenId);
}
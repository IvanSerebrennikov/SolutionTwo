using System.Security.Claims;
using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Identity.Models.Auth.Incoming;
using SolutionTwo.Business.Identity.Models.Auth.Outgoing;

namespace SolutionTwo.Business.Identity.Services.Interfaces;

public interface IIdentityService
{
    Task<IServiceResult<AuthResult>> ValidateCredentialsAndCreateTokensPairAsync(
        UserCredentialsModel userCredentials);
    
    Task<IServiceResult<TokensPairModel>> RefreshTokensPairAsync(string refreshToken);
    
    IServiceResult<ClaimsPrincipal> VerifyAuthTokenAndGetPrincipal(string authToken);

    Task RevokeAllActiveTokensForUserAsync(Guid userId);
    
    Task DeactivateUserAsync(Guid userId);
}
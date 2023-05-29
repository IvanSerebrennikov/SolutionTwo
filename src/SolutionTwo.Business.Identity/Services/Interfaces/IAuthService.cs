using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Identity.Models.Auth.Incoming;
using SolutionTwo.Business.Identity.Models.Auth.Outgoing;

namespace SolutionTwo.Business.Identity.Services.Interfaces;

public interface IAuthService
{
    Task<IServiceResult<TokensPairModel>> CreateTokensPairAsync(UserCredentialsModel userCredentials);
    
    Task<IServiceResult<TokensPairModel>> RefreshTokensPairAsync(Guid refreshToken);
    
    bool IsAuthTokenRevoked(Guid authTokenId);
}
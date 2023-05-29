using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Identity.Models.Auth.Outgoing;

namespace SolutionTwo.Business.Identity.Services.Interfaces;

public interface IAuthService
{
    Task<IServiceResult<TokensPairModel>> CreateTokensPairAsync(Guid userId);
    
    Task<IServiceResult<TokensPairModel>> RefreshTokensPairAsync(Guid refreshToken);
    
    bool IsAuthTokenRevoked(Guid authTokenId);
}
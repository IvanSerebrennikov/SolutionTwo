using SolutionTwo.Business.Models;
using SolutionTwo.Business.Models.Auth.Incoming;
using SolutionTwo.Business.Models.Auth.Outgoing;

namespace SolutionTwo.Business.Services.Interfaces;

public interface IAuthService
{
    Task<IServiceResult<TokensPairModel>> CreateTokensPairAsync(UserCredentialsModel userCredentials);
    
    Task<IServiceResult<TokensPairModel>> RefreshTokensPairAsync(Guid refreshToken);
}
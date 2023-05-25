using SolutionTwo.Domain.Models;
using SolutionTwo.Domain.Models.Auth.Incoming;
using SolutionTwo.Domain.Models.Auth.Outgoing;

namespace SolutionTwo.Domain.Services.Interfaces;

public interface IAuthService
{
    Task<IServiceResult<TokensPairModel>> CreateTokensPairAsync(UserCredentialsModel userCredentials);
    
    Task<IServiceResult<TokensPairModel>> RefreshTokensPairAsync(Guid refreshToken);
}
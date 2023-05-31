using System.Security.Claims;
using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Identity.Models.Auth.Outgoing;

namespace SolutionTwo.Business.Identity.Services.Interfaces;

public interface IAuthService
{
    Task<IServiceResult<TokensPairModel>> CreateTokensPairAsync(Guid userId);
    
    Task<IServiceResult<TokensPairModel>> RefreshTokensPairAsync(string refreshToken);
    
    IServiceResult<ClaimsPrincipal> VerifyAuthTokenAndGetPrincipal(string tokenString);
}
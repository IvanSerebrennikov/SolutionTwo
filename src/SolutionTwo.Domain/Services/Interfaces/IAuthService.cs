using SolutionTwo.Domain.Models.Auth;
using SolutionTwo.Domain.Models.Auth.Outgoing;

namespace SolutionTwo.Domain.Services.Interfaces;

public interface IAuthService
{
    Task<string> CreateRefreshTokenForUserAsync(Guid userId);

    Task<RefreshTokenModel?> GetRefreshTokenAsync(string tokenValue);

    Task<string> MarkRefreshTokenAsUsedAndCreateNewOneAsync(Guid tokenId);

    Task RevokeProvidedAndAllActiveRefreshTokensForUserAsync(Guid tokenId, Guid userId);
}
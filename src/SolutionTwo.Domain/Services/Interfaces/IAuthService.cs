using SolutionTwo.Domain.Models.Auth;

namespace SolutionTwo.Domain.Services.Interfaces;

public interface IAuthService
{
    Task<string> CreateRefreshTokenForUserAsync(Guid userId);

    Task<RefreshTokenModel?> GetRefreshTokenAsync(string tokenValue);

    Task<string> MarkRefreshTokenAsUsedAndCreateNewAsync(Guid tokenId);

    Task RevokeProvidedAndAllActiveRefreshTokensForUser(Guid tokenId, Guid userId);
}
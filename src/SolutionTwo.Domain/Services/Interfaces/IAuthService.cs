using SolutionTwo.Domain.Models.Auth;
using SolutionTwo.Domain.Models.Auth.Outgoing;

namespace SolutionTwo.Domain.Services.Interfaces;

public interface IAuthService
{
    Task<Guid> CreateRefreshTokenForUserAsync(Guid userId, Guid authTokenId);

    Task<RefreshTokenModel?> GetRefreshTokenAsync(Guid tokenId);

    Task<Guid> MarkRefreshTokenAsUsedAndCreateNewOneAsync(Guid tokenId, Guid authTokenId);

    Task RevokeProvidedAndAllActiveRefreshTokensForUserAsync(Guid tokenId, Guid userId);
}
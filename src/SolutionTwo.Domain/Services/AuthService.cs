using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Interfaces;
using SolutionTwo.Data.UnitOfWork.Interfaces;
using SolutionTwo.Domain.Models.Auth.Outgoing;
using SolutionTwo.Domain.Services.Interfaces;
using SolutionTwo.Identity.TokenManaging.Interfaces;

namespace SolutionTwo.Domain.Services;

public class AuthService : IAuthService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IMainDatabase _mainDatabase;
    private readonly ITokenManager _tokenManager;

    public AuthService(
        IRefreshTokenRepository refreshTokenRepository, 
        IMainDatabase mainDatabase, 
        ITokenManager tokenManager)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _mainDatabase = mainDatabase;
        _tokenManager = tokenManager;
    }

    public async Task<Guid> CreateRefreshTokenForUserAsync(Guid userId, Guid authTokenId)
    {
        var refreshTokenValue = CreateRefreshToken(userId, authTokenId);
        await _mainDatabase.CommitChangesAsync();

        return refreshTokenValue;
    }

    public async Task<RefreshTokenModel?> GetRefreshTokenAsync(Guid tokenId)
    {
        var tokenEntity = await _refreshTokenRepository.GetByIdAsync(tokenId);

        return tokenEntity != null ? new RefreshTokenModel(tokenEntity) : null;
    }

    public async Task<Guid> MarkRefreshTokenAsUsedAndCreateNewOneAsync(Guid tokenId, Guid authTokenId)
    {
        var tokenEntity = await _refreshTokenRepository.GetByIdAsync(tokenId);

        if (tokenEntity == null)
        {
            throw new ApplicationException($"Refresh token with id {tokenId} was not found.");
        }
        
        tokenEntity.IsUsed = true;
            
        var newRefreshTokenId = CreateRefreshToken(tokenEntity.UserId, authTokenId);
            
        await _mainDatabase.CommitChangesAsync();

        return newRefreshTokenId;
    }

    public async Task RevokeProvidedAndAllActiveRefreshTokensForUserAsync(Guid tokenId, Guid userId)
    {
        var tokenEntities = await _refreshTokenRepository.GetAsync(
            x => x.Id == tokenId || (
                x.UserId == userId && !x.IsUsed && x.ExpiresDateTimeUtc > DateTime.UtcNow));

        foreach (var tokenEntity in tokenEntities)
        {
            RevokeToken(tokenEntity);
        }

        await _mainDatabase.CommitChangesAsync();
    }

    private Guid CreateRefreshToken(Guid userId, Guid authTokenId)
    {
        var refreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            AuthTokenId = authTokenId,
            UserId = userId,
            CreatedDateTimeUtc = DateTime.UtcNow,
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(7)
        };

        _refreshTokenRepository.Create(refreshToken);

        return refreshToken.Id;
    }

    private void RevokeToken(RefreshTokenEntity refreshTokenEntity)
    {
        refreshTokenEntity.IsRevoked = true;
        _refreshTokenRepository.Update(refreshTokenEntity);
        _tokenManager.DeactivateToken(refreshTokenEntity.AuthTokenId);
    }
}
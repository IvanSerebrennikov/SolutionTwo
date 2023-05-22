using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Interfaces;
using SolutionTwo.Data.UnitOfWork.Interfaces;
using SolutionTwo.Domain.Models.Auth;
using SolutionTwo.Domain.Services.Interfaces;

namespace SolutionTwo.Domain.Services;

public class AuthService : IAuthService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IMainDatabase _mainDatabase;

    public AuthService(IRefreshTokenRepository refreshTokenRepository, IMainDatabase mainDatabase)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _mainDatabase = mainDatabase;
    }

    public async Task<string> CreateRefreshTokenForUserAsync(Guid userId)
    {
        var refreshTokenValue = CreateRefreshToken(userId);
        await _mainDatabase.CommitChangesAsync();

        return refreshTokenValue;
    }

    public async Task<RefreshTokenModel?> GetRefreshTokenAsync(string tokenValue)
    {
        var tokenEntity = await _refreshTokenRepository.GetSingleAsync(x => x.Value == tokenValue);

        return tokenEntity != null ? new RefreshTokenModel(tokenEntity) : null;
    }

    public async Task<string> MarkRefreshTokenAsUsedAndCreateNewAsync(Guid tokenId)
    {
        var tokenEntity = await _refreshTokenRepository.GetByIdAsync(tokenId);

        if (tokenEntity != null)
        {
            tokenEntity.IsUsed = true;
            
            var newRefreshTokenValue = CreateRefreshToken(tokenEntity.UserId);
            
            await _mainDatabase.CommitChangesAsync();

            return newRefreshTokenValue;
        }

        return "";
    }

    public async Task RevokeProvidedAndAllActiveRefreshTokensForUserAsync(Guid tokenId, Guid userId)
    {
        var tokenEntities = await _refreshTokenRepository.GetAsync(
            x => x.Id == tokenId || (
                x.UserId == userId && !x.IsUsed && x.ExpiresDateTimeUtc > DateTime.UtcNow));

        foreach (var tokenEntity in tokenEntities)
        {
            tokenEntity.IsRevoked = true;
            _refreshTokenRepository.Update(tokenEntity);
        }

        await _mainDatabase.CommitChangesAsync();
    }

    private string CreateRefreshToken(Guid userId)
    {
        var refreshToken = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            Value = $"{Guid.NewGuid()}-{Guid.NewGuid()}",
            UserId = userId,
            CreatedDateTimeUtc = DateTime.UtcNow,
            ExpiresDateTimeUtc = DateTime.UtcNow.AddDays(7)
        };

        _refreshTokenRepository.Create(refreshToken);

        return refreshToken.Value;
    }
}
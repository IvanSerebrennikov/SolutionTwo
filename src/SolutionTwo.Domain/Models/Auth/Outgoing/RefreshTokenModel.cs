using SolutionTwo.Data.Entities;

namespace SolutionTwo.Domain.Models.Auth.Outgoing;

public class RefreshTokenModel
{
    public RefreshTokenModel(RefreshTokenEntity refreshTokenEntity)
    {
        Id = refreshTokenEntity.Id;
        IsUsed = refreshTokenEntity.IsUsed;
        IsRevoked = refreshTokenEntity.IsRevoked;
        IsExpired = refreshTokenEntity.ExpiresDateTimeUtc <= DateTime.UtcNow;
        UserId = refreshTokenEntity.UserId;
    }
    
    public Guid Id { get; private set; }
    
    public bool IsUsed { get; private set; }

    public bool IsRevoked { get; private set; }
    
    public bool IsExpired { get; private set; }

    public Guid UserId { get; private set; }
}
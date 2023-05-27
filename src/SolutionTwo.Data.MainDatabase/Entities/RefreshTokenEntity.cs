using SolutionTwo.Data.Common.Entities.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Entities;

public class RefreshTokenEntity : IIdentifiablyEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid AuthTokenId { get; set; }

    public bool IsUsed { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime ExpiresDateTimeUtc { get; set; }

    public Guid UserId { get; set; }

    public UserEntity? User { get; set; }
}
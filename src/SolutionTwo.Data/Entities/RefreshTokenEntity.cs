using System.ComponentModel.DataAnnotations;
using SolutionTwo.Data.Entities.Base.Interfaces;

namespace SolutionTwo.Data.Entities;

public class RefreshTokenEntity : IIdentifiablyEntity<Guid>
{
    public Guid Id { get; set; }

    [MaxLength(256)]
    public string Value { get; set; } = null!;

    public bool IsUsed { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime ExpiresDateTimeUtc { get; set; }

    public Guid UserId { get; set; }

    public UserEntity? User { get; set; }
}
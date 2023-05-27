using System.ComponentModel.DataAnnotations;
using SolutionTwo.Data.Common.Entities.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Entities;

public class UserEntity : IIdentifiablyEntity<Guid>
{
    public Guid Id { get; set; }

    [MaxLength(256)]
    public string FirstName { get; set; } = null!;
    
    [MaxLength(256)]
    public string LastName { get; set; } = null!;

    [MaxLength(256)]
    public string Username { get; set; } = null!;

    [MaxLength(1024)]
    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }
    
    public List<RoleEntity> Roles { get; set; } = new();
    
    public List<RefreshTokenEntity> RefreshTokens { get; set; } = new();
}
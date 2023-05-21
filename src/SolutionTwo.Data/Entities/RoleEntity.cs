using System.ComponentModel.DataAnnotations;
using SolutionTwo.Data.Entities.Base.Interfaces;

namespace SolutionTwo.Data.Entities;

public class RoleEntity : IIdentifiablyEntity<Guid>
{
    public Guid Id { get; set; }
    
    [MaxLength(64)]
    public string Name { get; set; } = null!;

    public List<UserEntity> Users { get; set; } = new();
}
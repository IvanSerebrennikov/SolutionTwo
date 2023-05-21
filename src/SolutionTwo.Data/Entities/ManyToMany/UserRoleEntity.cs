namespace SolutionTwo.Data.Entities.ManyToMany;

public class UserRoleEntity
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }
}
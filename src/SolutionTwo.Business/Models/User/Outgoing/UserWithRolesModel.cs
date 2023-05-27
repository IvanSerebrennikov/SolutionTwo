using SolutionTwo.Data.Entities;

namespace SolutionTwo.Business.Models.User.Outgoing;

public class UserWithRolesModel
{
    public UserWithRolesModel(UserEntity userEntity)
    {
        Id = userEntity.Id;
        FirstName = userEntity.FirstName;
        LastName = userEntity.LastName;
        CreatedDateTimeUtc = userEntity.CreatedDateTimeUtc;
        Username = userEntity.Username;
        Roles = userEntity.Roles.Select(x => x.Name).ToList();
    }

    public Guid Id { get; private set; }
    
    public string FirstName { get; private set; }
    
    public string LastName { get; private set; }
    
    public string Username { get; private set; }
    
    public DateTime CreatedDateTimeUtc { get; private set; }
    
    public List<string> Roles { get; private set; }
}
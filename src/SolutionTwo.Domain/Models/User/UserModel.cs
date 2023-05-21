using SolutionTwo.Data.Entities;

namespace SolutionTwo.Domain.Models.User;

public class UserModel
{
    public UserModel(UserEntity userEntity)
    {
        Id = userEntity.Id;
        FirstName = userEntity.FirstName;
        LastName = userEntity.LastName;
        CreatedDateTimeUtc = userEntity.CreatedDateTimeUtc;
    }

    public Guid Id { get; private set; }
    
    public string FirstName { get; private set; }
    
    public string LastName { get; private set; }
    
    public DateTime CreatedDateTimeUtc { get; private set; }
}
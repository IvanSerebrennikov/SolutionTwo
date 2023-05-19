using SolutionTwo.Data.Entities;

namespace SolutionTwo.Domain.Models.User;

public class UserModel
{
    public UserModel(UserEntity userEntity)
    {
        Id = userEntity.Id;
        FirstName = userEntity.FirstName;
    }

    public Guid Id { get; set; }
    
    public string? FirstName { get; set; }
}
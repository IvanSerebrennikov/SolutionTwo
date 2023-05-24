using SolutionTwo.Data.Entities;

namespace SolutionTwo.Domain.Models.User.Read;

public class UserAuthModel
{
    public UserAuthModel(UserEntity userEntity)
    {
        UserData = new UserWithRolesModel(userEntity);
        PasswordHash = userEntity.PasswordHash;
    }

    public UserWithRolesModel UserData { get; private set; }

    public string PasswordHash { get; private set; }
}
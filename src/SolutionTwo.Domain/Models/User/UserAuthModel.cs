using SolutionTwo.Data.Entities;

namespace SolutionTwo.Domain.Models.User;

public class UserAuthModel
{
    public UserAuthModel(UserEntity userEntity)
    {
        UserData = new UserModel(userEntity);
        Roles = userEntity.Roles.Select(x => x.Name).ToList();
        PasswordHash = userEntity.PasswordHash;
    }

    public UserModel UserData { get; private set; }

    public string PasswordHash { get; private set; }

    public List<string> Roles { get; private set; }
}
using SolutionTwo.Data.InMemory.Common;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.InMemory.MainDatabase;

public class InMemoryUserRepository : BaseInMemoryRepository<UserEntity, Guid>, IUserRepository
{
    public void AddUserToRole(UserEntity user, RoleEntity role)
    {
        if (user.Roles.All(x => x.Id != role.Id))
        {
            user.Roles.Add(role);
        }
    }
}
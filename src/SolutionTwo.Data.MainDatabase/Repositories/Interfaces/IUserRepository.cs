using SolutionTwo.Data.Common.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;

namespace SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

public interface IUserRepository : IBaseRepository<UserEntity, Guid>
{
    void AddUserToRole(UserEntity user, RoleEntity role);
}
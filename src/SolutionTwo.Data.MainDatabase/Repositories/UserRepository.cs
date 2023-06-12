using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Entities.ManyToMany;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Repositories;

public class UserRepository : BaseRepository<MainDatabaseContext, UserEntity, Guid>, IUserRepository
{
    public UserRepository(MainDatabaseContext context) : base(context)
    {
    }

    public void AddUserToRole(UserEntity user, RoleEntity role)
    {
        Context.Set<UserRoleRelation>().Add(new UserRoleRelation
        {
            RoleId = role.Id,
            UserId = user.Id
        });
    }
}
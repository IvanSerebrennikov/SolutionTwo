using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Entities.ManyToMany;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Repositories;

public class RoleRepository : BaseRepository<MainDatabaseContext, RoleEntity, Guid>, IRoleRepository
{
    public RoleRepository(MainDatabaseContext context) : base(context)
    {
    }

    public void AddRoleToUser(RoleEntity role, UserEntity user)
    {
        Context.Set<UserRoleRelation>().Add(new UserRoleRelation
        {
            RoleId = role.Id,
            UserId = user.Id
        });
    }
}
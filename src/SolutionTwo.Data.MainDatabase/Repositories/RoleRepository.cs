using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Repositories;

public class RoleRepository : BaseRepository<MainDatabaseContext, RoleEntity, Guid>, IRoleRepository
{
    public RoleRepository(MainDatabaseContext context) : base(context)
    {
    }
}
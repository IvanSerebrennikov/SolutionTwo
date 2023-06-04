using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Repositories;

public class TenantRepository : BaseRepository<MainDatabaseContext, TenantEntity, Guid>, ITenantRepository
{
    public TenantRepository(MainDatabaseContext context) : base(context)
    {
    }
}
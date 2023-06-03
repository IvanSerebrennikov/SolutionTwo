using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.Common.MultiTenant.Repositories;
using SolutionTwo.MultiTenancy;

namespace SolutionTwo.Data.MainDatabase.Repositories;

public class UserRepository : BaseMultiTenantRepository<UserEntity, Guid>, IUserRepository
{
    public UserRepository(MainDatabaseContext context, ITenantAccessGetter tenantAccessGetter) : base(context,
        tenantAccessGetter)
    {
    }
}
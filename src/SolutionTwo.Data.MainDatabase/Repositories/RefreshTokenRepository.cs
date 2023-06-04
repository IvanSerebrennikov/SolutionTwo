using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Repositories;

public class RefreshTokenRepository : BaseRepository<MainDatabaseContext, RefreshTokenEntity, Guid>,
    IRefreshTokenRepository
{
    public RefreshTokenRepository(MainDatabaseContext context) : base(context)
    {
    }
}
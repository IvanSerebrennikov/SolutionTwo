using SolutionTwo.Data.Context;
using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Base;
using SolutionTwo.Data.Repositories.Interfaces;

namespace SolutionTwo.Data.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshTokenEntity, Guid>, IRefreshTokenRepository
{
    public RefreshTokenRepository(MainDatabaseContext context) : base(context)
    {
    }
}
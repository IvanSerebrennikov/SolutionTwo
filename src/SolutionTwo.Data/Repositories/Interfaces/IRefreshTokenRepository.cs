using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Base.Interfaces;

namespace SolutionTwo.Data.Repositories.Interfaces;

public interface IRefreshTokenRepository : IBaseRepository<RefreshTokenEntity, Guid>
{
    
}
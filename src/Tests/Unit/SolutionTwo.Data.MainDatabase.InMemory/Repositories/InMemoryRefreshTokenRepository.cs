using SolutionTwo.Data.Common.InMemory;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.InMemory;

public class InMemoryRefreshTokenRepository : BaseInMemoryRepository<RefreshTokenEntity, Guid>, IRefreshTokenRepository
{
    
}
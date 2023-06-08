using SolutionTwo.Data.InMemory.Common;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.InMemory.MainDatabase;

public class InMemoryRefreshTokenRepository : BaseInMemoryRepository<RefreshTokenEntity, Guid>, IRefreshTokenRepository
{
    
}
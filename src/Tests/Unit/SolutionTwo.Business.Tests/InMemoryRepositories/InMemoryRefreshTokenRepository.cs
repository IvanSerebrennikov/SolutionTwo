using SolutionTwo.Business.Tests.InMemoryRepositories.Base;
using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Interfaces;

namespace SolutionTwo.Business.Tests.InMemoryRepositories;

public class InMemoryRefreshTokenRepository : BaseInMemoryRepository<RefreshTokenEntity, Guid>, IRefreshTokenRepository
{
    
}
using SolutionTwo.Business.Tests.InMemoryRepositories.Base;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Business.Tests.InMemoryRepositories;

public class InMemoryRefreshTokenRepository : BaseInMemoryRepository<RefreshTokenEntity, Guid>, IRefreshTokenRepository
{
    
}
using SolutionTwo.Business.Tests.InMemoryRepositories.Base;
using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Interfaces;

namespace SolutionTwo.Business.Tests.InMemoryRepositories;

public class InMemoryUserRepository : BaseInMemoryRepository<UserEntity, Guid>, IUserRepository
{
    
}
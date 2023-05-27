using SolutionTwo.Business.Tests.InMemoryRepositories.Base;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Business.Tests.InMemoryRepositories;

public class InMemoryUserRepository : BaseInMemoryRepository<UserEntity, Guid>, IUserRepository
{
    
}
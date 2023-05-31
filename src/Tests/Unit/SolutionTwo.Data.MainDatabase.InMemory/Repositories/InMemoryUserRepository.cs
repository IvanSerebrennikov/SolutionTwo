using SolutionTwo.Data.Common.InMemory.Repositories;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.InMemory.Repositories;

public class InMemoryUserRepository : BaseInMemoryRepository<UserEntity, Guid>, IUserRepository
{
    
}
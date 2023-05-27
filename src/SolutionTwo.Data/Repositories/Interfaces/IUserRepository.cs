using SolutionTwo.Data.Common.Repositories.Interfaces;
using SolutionTwo.Data.Entities;

namespace SolutionTwo.Data.Repositories.Interfaces;

public interface IUserRepository : IBaseRepository<UserEntity, Guid>
{
    
}
using SolutionTwo.Data.Context;
using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Base;
using SolutionTwo.Data.Repositories.Interfaces;

namespace SolutionTwo.Data.Repositories;

public class UserRepository : BaseRepository<User, Guid>, IUserRepository
{
    public UserRepository(MainDatabaseContext context) : base(context)
    {
    }
}
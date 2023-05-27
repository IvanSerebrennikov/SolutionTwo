using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.Data.MainDatabase.Context;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;

namespace SolutionTwo.Data.MainDatabase.Repositories;

public class UserRepository : BaseRepository<UserEntity, Guid>, IUserRepository
{
    public UserRepository(MainDatabaseContext context) : base(context)
    {
    }
}
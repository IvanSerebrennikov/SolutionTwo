﻿using SolutionTwo.Data.Common.Repositories;
using SolutionTwo.Data.Context;
using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Interfaces;

namespace SolutionTwo.Data.Repositories;

public class UserRepository : BaseRepository<UserEntity, Guid>, IUserRepository
{
    public UserRepository(MainDatabaseContext context) : base(context)
    {
    }
}
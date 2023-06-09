﻿using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Core.Models.User.Incoming;
using SolutionTwo.Business.Core.Models.User.Outgoing;

namespace SolutionTwo.Business.Core.Services.Interfaces;

public interface IUserService
{
    Task<bool> UserExistsAsync(Guid id);
    
    Task<UserWithRolesModel?> GetUserWithRolesByIdAsync(Guid id);
    
    Task<IReadOnlyList<UserWithRolesModel>> GetAllUsersWithRolesAsync();

    Task<IServiceResult<UserWithRolesModel>> CreateTenantUserAsync(CreateUserModel createUserModel);

    Task<IServiceResult> DeleteUserAsync(Guid id);
    
    Task DeactivateUserAsync(Guid userId);
}
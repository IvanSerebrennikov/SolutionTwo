using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Core.Models.User.Incoming;
using SolutionTwo.Business.Core.Models.User.Outgoing;

namespace SolutionTwo.Business.Core.Services.Interfaces;

public interface IUserService
{
    Task<UserWithRolesModel?> GetUserWithRolesByIdAsync(Guid id);

    Task<UserWithRolesModel?> GetUserWithRolesAsync(string username);
    
    Task<IReadOnlyList<UserWithRolesModel>> GetAllUsersWithRolesAsync();

    Task<UserWithRolesModel> CreateUserAsync(CreateUserModel createUserModel, Guid tenantId);

    Task<IServiceResult> DeleteUserAsync(Guid id);
}
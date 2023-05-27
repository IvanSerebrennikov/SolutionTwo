using SolutionTwo.Business.Models.User.Incoming;
using SolutionTwo.Business.Models.User.Outgoing;

namespace SolutionTwo.Business.Services.Interfaces;

public interface IUserService
{
    Task<UserWithRolesModel?> GetUserWithRolesByIdAsync(Guid id);

    Task<UserWithRolesModel?> GetUserWithRolesAsync(string username);

    Task<IReadOnlyList<UserWithRolesModel>> GetAllUsersWithRolesAsync();

    Task<UserWithRolesModel> AddUserAsync(CreateUserModel createUserModel);
}
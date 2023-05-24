using SolutionTwo.Domain.Models.User;
using SolutionTwo.Domain.Models.User.Incoming;
using SolutionTwo.Domain.Models.User.Outgoing;

namespace SolutionTwo.Domain.Services.Interfaces;

public interface IUserService
{
    Task<UserWithRolesModel?> GetUserWithRolesByIdAsync(Guid id);

    Task<UserAuthModel?> GetUserWithRolesAsync(string username);

    Task<IReadOnlyList<UserWithRolesModel>> GetAllUsersWithRolesAsync();

    Task<UserWithRolesModel> AddUserAsync(CreateUserModel createUserModel);
}
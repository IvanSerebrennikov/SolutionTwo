using SolutionTwo.Domain.Models.User;
using SolutionTwo.Domain.Models.User.Input;
using SolutionTwo.Domain.Models.User.Output;

namespace SolutionTwo.Domain.Services.Interfaces;

public interface IUserService
{
    Task<UserWithRolesModel?> GetUserWithRolesByIdAsync(Guid id);

    Task<UserAuthModel?> GetUserWithRolesAsync(string username);

    Task<IReadOnlyList<UserWithRolesModel>> GetAllUsersWithRolesAsync();

    Task<UserWithRolesModel> AddUserAsync(CreateUserModel createUserModel);
}
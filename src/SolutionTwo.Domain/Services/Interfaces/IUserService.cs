using SolutionTwo.Domain.Models.User;
using SolutionTwo.Domain.Models.User.Read;
using SolutionTwo.Domain.Models.User.Write;

namespace SolutionTwo.Domain.Services.Interfaces;

public interface IUserService
{
    Task<UserWithRolesModel?> GetUserWithRolesByIdAsync(Guid id);

    Task<UserAuthModel?> GetUserWithRolesAsync(string username);

    Task<IReadOnlyList<UserWithRolesModel>> GetAllUsersWithRolesAsync();

    Task<UserWithRolesModel> AddUserAsync(CreateUserModel createUserModel);
}
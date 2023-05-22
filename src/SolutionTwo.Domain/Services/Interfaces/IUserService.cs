using SolutionTwo.Domain.Models.User;

namespace SolutionTwo.Domain.Services.Interfaces;

public interface IUserService
{
    Task<UserWithRolesModel?> GetUserWithRolesByIdAsync(Guid id);

    Task<UserAuthModel?> GetUserWithRolesAsync(string username);

    Task<IReadOnlyList<UserWithRolesModel>> GetAllUsersWithRolesAsync();

    Task<UserWithRolesModel> AddUserAsync(UserCreationModel userCreationModel);
}
using SolutionTwo.Domain.Models.User;

namespace SolutionTwo.Domain.Services.Interfaces;

public interface IUserService
{
    Task<UserModel?> GetUserAsync(Guid id);

    Task<IReadOnlyList<UserModel>> GetAllUsersAsync();

    Task<UserModel> AddUserAsync(UserCreationModel userCreationModel);
}
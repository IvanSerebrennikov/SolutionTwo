using System.Runtime.CompilerServices;
using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Interfaces;
using SolutionTwo.Data.UnitOfWork.Interfaces;
using SolutionTwo.Domain.Models.User;
using SolutionTwo.Domain.Services.Interfaces;

namespace SolutionTwo.Domain.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMainDatabase _mainDatabase;

    public UserService(IMainDatabase mainDatabase, IUserRepository userRepository)
    {
        _mainDatabase = mainDatabase;
        _userRepository = userRepository;
    }

    public async Task<UserModel?> GetUserAsync(Guid id)
    {
        var userEntity = await _userRepository.GetByIdAsync(id);

        return userEntity != null ? new UserModel(userEntity) : null;
    }

    public async Task<IReadOnlyList<UserModel>> GetAllUsersAsync()
    {
        var userEntities = await _userRepository.GetAsync();
        var userModels = userEntities.Select(x => new UserModel(x)).ToList();

        return userModels;
    }

    public async Task<UserModel> AddUserAsync(UserCreationModel userCreationModel)
    {
        AssertValueIsNotNull(nameof(userCreationModel.FirstName), userCreationModel.FirstName);
        var firstName = userCreationModel.FirstName!;
        
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = firstName
        };

        await _userRepository.CreateAsync(userEntity);
        await _mainDatabase.CommitChangesAsync();

        return new UserModel(userEntity);
    }

    private static void AssertValueIsNotNull<T>(string name, T value, [CallerMemberName] string caller = "")
    {
        if (value == null)
        {
            throw new ArgumentException($"Assertion failed in {caller}. Value of {name} is null.");
        }
    }
}
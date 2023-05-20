using System.Runtime.CompilerServices;
using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Interfaces;
using SolutionTwo.Data.UnitOfWork.Interfaces;
using SolutionTwo.Domain.Models.User;
using SolutionTwo.Domain.Services.Interfaces;
using SolutionTwo.Identity.PasswordProcessing.Interfaces;

namespace SolutionTwo.Domain.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMainDatabase _mainDatabase;
    private readonly IPasswordProcessor _passwordProcessor;

    public UserService(IMainDatabase mainDatabase, IUserRepository userRepository, IPasswordProcessor passwordProcessor)
    {
        _mainDatabase = mainDatabase;
        _userRepository = userRepository;
        _passwordProcessor = passwordProcessor;
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
        var hashedPassword = _passwordProcessor.HashPassword(userCreationModel, userCreationModel.Password!);

        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = userCreationModel.FirstName!
        };

        await _userRepository.CreateAsync(userEntity);
        await _mainDatabase.CommitChangesAsync();

        return new UserModel(userEntity);
    }

    /// <summary>
    ///     Usage: AssertValueIsNotNull(nameof(model.SomeProperty), model.SomeProperty);
    ///     Result: Assertion failed in SomeMethod. Value of SomeProperty is null.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="caller"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ArgumentException"></exception>
    private static void AssertValueIsNotNull<T>(string name, T value, [CallerMemberName] string caller = "")
    {
        if (value == null) throw new ArgumentException($"Assertion failed in {caller}. Value of {name} is null.");
    }
}
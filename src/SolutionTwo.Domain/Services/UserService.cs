using System.Linq.Expressions;
using SolutionTwo.Common.Extensions;
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

    public async Task<UserAuthModel?> GetUserWithRolesAsync(string username)
    {
        var userEntity = await _userRepository.GetOneAsync(
            x => x.Username == username,
            includeProperties: "Roles", asNoTracking: true);

        return userEntity != null ? new UserAuthModel(userEntity) : null;
    }

    public async Task<IReadOnlyList<UserModel>> GetAllUsersAsync()
    {
        var userEntities = await _userRepository.GetAsync();
        var userModels = userEntities.Select(x => new UserModel(x)).ToList();

        return userModels;
    }

    public async Task<UserModel> AddUserAsync(UserCreationModel userCreationModel)
    {
        userCreationModel.FirstName.AssertValueIsNotNull(nameof(userCreationModel.FirstName));
        userCreationModel.LastName.AssertValueIsNotNull(nameof(userCreationModel.LastName));
        userCreationModel.Username.AssertValueIsNotNull(nameof(userCreationModel.Username));
        userCreationModel.Password.AssertValueIsNotNull(nameof(userCreationModel.Password));

        var userId = Guid.NewGuid();
        var hashedPassword = _passwordProcessor.HashPassword(userId, userCreationModel.Password!);

        var userEntity = new UserEntity
        {
            Id = userId,
            FirstName = userCreationModel.FirstName!,
            LastName = userCreationModel.LastName!,
            Username = userCreationModel.Username!,
            PasswordHash = hashedPassword,
            CreatedDateTimeUtc = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(userEntity);
        await _mainDatabase.CommitChangesAsync();

        return new UserModel(userEntity);
    }
}
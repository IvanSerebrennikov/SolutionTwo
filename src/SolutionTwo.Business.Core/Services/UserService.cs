using SolutionTwo.Business.Common.PasswordManager.Interfaces;
using SolutionTwo.Business.Core.Models.User.Incoming;
using SolutionTwo.Business.Core.Models.User.Outgoing;
using SolutionTwo.Business.Core.Services.Interfaces;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.Repositories.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMainDatabase _mainDatabase;
    private readonly IPasswordManager _passwordManager;

    public UserService(
        IMainDatabase mainDatabase, 
        IUserRepository userRepository, 
        IPasswordManager passwordManager)
    {
        _mainDatabase = mainDatabase;
        _userRepository = userRepository;
        _passwordManager = passwordManager;
    }

    public async Task<UserWithRolesModel?> GetUserWithRolesByIdAsync(Guid id)
    {
        var userEntity = await _userRepository.GetByIdAsync(id, includeProperties: "Roles", asNoTracking: true);

        return userEntity != null ? new UserWithRolesModel(userEntity) : null;
    }

    public async Task<UserWithRolesModel?> GetUserWithRolesAsync(string username)
    {
        var userEntity = await _userRepository.GetSingleAsync(
            x => x.Username == username,
            includeProperties: "Roles", asNoTracking: true);

        return userEntity != null ? new UserWithRolesModel(userEntity) : null;
    }

    public async Task<IReadOnlyList<UserWithRolesModel>> GetAllUsersWithRolesAsync()
    {
        var userEntities = await _userRepository.GetAsync(includeProperties: "Roles", asNoTracking: true);
        var userModels = userEntities.Select(x => new UserWithRolesModel(x)).ToList();

        return userModels;
    }

    public async Task<UserWithRolesModel> AddUserAsync(CreateUserModel createUserModel)
    {
        createUserModel.FirstName.AssertValueIsNotNull();
        createUserModel.LastName.AssertValueIsNotNull();
        createUserModel.Username.AssertValueIsNotNull();
        createUserModel.Password.AssertValueIsNotNull();

        var hashedPassword = _passwordManager.HashPassword(createUserModel.Password!);

        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = createUserModel.FirstName!,
            LastName = createUserModel.LastName!,
            Username = createUserModel.Username!,
            PasswordHash = hashedPassword,
            CreatedDateTimeUtc = DateTime.UtcNow
        };

        _userRepository.Create(userEntity);
        await _mainDatabase.CommitChangesAsync();

        return new UserWithRolesModel(userEntity);
    }
}
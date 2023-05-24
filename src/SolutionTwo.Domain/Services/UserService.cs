using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.Entities;
using SolutionTwo.Data.Repositories.Interfaces;
using SolutionTwo.Data.UnitOfWork.Interfaces;
using SolutionTwo.Domain.Models.User;
using SolutionTwo.Domain.Models.User.Read;
using SolutionTwo.Domain.Models.User.Write;
using SolutionTwo.Domain.Services.Interfaces;
using SolutionTwo.Identity.PasswordManaging.Interfaces;

namespace SolutionTwo.Domain.Services;

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

    public async Task<UserAuthModel?> GetUserWithRolesAsync(string username)
    {
        var userEntity = await _userRepository.GetSingleAsync(
            x => x.Username == username,
            includeProperties: "Roles", asNoTracking: true);

        return userEntity != null ? new UserAuthModel(userEntity) : null;
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
using Microsoft.Extensions.Logging;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;
using SolutionTwo.Business.Common.ValueAssertion;
using SolutionTwo.Business.Core.Models.User.Incoming;
using SolutionTwo.Business.Core.Models.User.Outgoing;
using SolutionTwo.Business.Core.Services.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Business.Core.Services;

public class UserService : IUserService
{
    private readonly IMainDatabase _mainDatabase;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IMainDatabase mainDatabase, 
        IPasswordHasher passwordHasher, 
        ILogger<UserService> logger)
    {
        _mainDatabase = mainDatabase;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserWithRolesModel?> GetUserWithRolesByIdAsync(Guid id)
    {
        var userEntity = await _mainDatabase.Users.GetByIdAsync(id, includeProperties: "Roles", asNoTracking: true);

        return userEntity != null ? new UserWithRolesModel(userEntity) : null;
    }

    public async Task<UserWithRolesModel?> GetUserWithRolesAsync(string username)
    {
        var userEntity = await _mainDatabase.Users.GetSingleAsync(
            x => x.Username == username,
            includeProperties: "Roles", 
            asNoTracking: true);

        return userEntity != null ? new UserWithRolesModel(userEntity) : null;
    }

    public async Task<IReadOnlyList<UserWithRolesModel>> GetAllUsersWithRolesAsync()
    {
        var userEntities = await _mainDatabase.Users.GetAsync(includeProperties: "Roles", asNoTracking: true);
        var userModels = userEntities.Select(x => new UserWithRolesModel(x)).ToList();

        return userModels;
    }

    public async Task<UserWithRolesModel> AddUserAsync(CreateUserModel createUserModel)
    {
        createUserModel.FirstName.AssertValueIsNotNull();
        createUserModel.LastName.AssertValueIsNotNull();
        createUserModel.Username.AssertValueIsNotNull();
        createUserModel.Password.AssertValueIsNotNull();

        var hashedPassword = _passwordHasher.HashPassword(createUserModel.Password!);

        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = createUserModel.FirstName!,
            LastName = createUserModel.LastName!,
            Username = createUserModel.Username!,
            PasswordHash = hashedPassword,
            CreatedDateTimeUtc = DateTime.UtcNow
        };

        _mainDatabase.Users.Create(userEntity);
        await _mainDatabase.CommitChangesAsync();

        return new UserWithRolesModel(userEntity);
    }
}
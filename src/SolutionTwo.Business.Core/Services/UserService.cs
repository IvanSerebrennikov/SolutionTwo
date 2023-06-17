using Microsoft.Extensions.Logging;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Business.Common.Models;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;
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

    public async Task<bool> UserExistsAsync(Guid id)
    {
        return await _mainDatabase.Users.ExistsAsync(x => x.Id == id);
    }

    public async Task<UserWithRolesModel?> GetUserWithRolesByIdAsync(Guid id)
    {
        var userEntity = await _mainDatabase.Users.GetByIdAsync(id, include: x => x.Roles);

        return userEntity != null ? new UserWithRolesModel(userEntity) : null;
    }

    public async Task<UserWithRolesModel?> GetUserWithRolesAsync(string username)
    {
        var userEntity = await _mainDatabase.Users.GetSingleAsync(
            x => x.Username == username,
            include: x => x.Roles);

        return userEntity != null ? new UserWithRolesModel(userEntity) : null;
    }

    public async Task<IReadOnlyList<UserWithRolesModel>> GetAllUsersWithRolesAsync()
    {
        var userEntities = await _mainDatabase.Users.GetAsync(include: x => x.Roles);
        var userModels = userEntities.Select(x => new UserWithRolesModel(x)).ToList();

        return userModels;
    }

    public async Task<IServiceResult<UserWithRolesModel>> CreateTenantUserAsync(CreateUserModel createUserModel)
    {
        var roleEntity = await _mainDatabase.Roles.GetSingleAsync(x => x.Name == UserRoles.TenantUser);
        if (roleEntity == null)
        {
            return ServiceResult<UserWithRolesModel>.Error($"Role '{UserRoles.TenantUser}' was not found");
        }
        
        var hashedPassword = _passwordHasher.HashPassword(createUserModel.Password);

        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = createUserModel.FirstName,
            LastName = createUserModel.LastName,
            Username = createUserModel.Username,
            PasswordHash = hashedPassword,
            CreatedDateTimeUtc = DateTime.UtcNow
        };

        _mainDatabase.Users.Create(userEntity);
        
        _mainDatabase.Users.AddUserToRole(userEntity, roleEntity);
        
        await _mainDatabase.CommitChangesAsync();

        var model = new UserWithRolesModel(userEntity);

        return ServiceResult<UserWithRolesModel>.Success(model);
    }

    public async Task<IServiceResult> DeleteUserAsync(Guid id)
    {
        var userEntity = await _mainDatabase.Users.GetByIdAsync(id);
        if (userEntity == null)
        {
            return ServiceResult.Error("User was not found");
        }
        
        _mainDatabase.Users.Delete(userEntity);
        await _mainDatabase.CommitChangesAsync();

        return ServiceResult.Success();
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}
using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Api.Controllers.Internal;

[Tags("_ApplicationSetUp")] // for swagger
[Route("api/[controller]")]
[ApiController]
public class ApplicationSetUpController : ApiControllerBase
{
    private readonly IMainDatabase _mainDatabase;
    private readonly IPasswordHasher _passwordHasher;

    public ApplicationSetUpController(IMainDatabase mainDatabase, IPasswordHasher passwordHasher)
    {
        _mainDatabase = mainDatabase;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("CreateRoles")]
    public async Task<ActionResult<Guid>> CreateRoles()
    {
        var roles = new[]
        {
            UserRoles.SuperAdmin,
            UserRoles.TenantAdmin,
            UserRoles.TenantUser
        };

        var created = 0;
        foreach (var role in roles)
        {
            var roleEntity = await _mainDatabase.Roles.GetSingleAsync(x => x.Name == role);
            if (roleEntity == null)
            {
                var newRoleEntity = new RoleEntity
                {
                    Id = Guid.NewGuid(),
                    Name = role
                };
                
                _mainDatabase.Roles.Create(newRoleEntity);

                created++;
            }
        }

        if (created > 0)
        {
            await _mainDatabase.CommitChangesAsync();
        }
        
        return Ok(created);
    }
    
    [HttpPost("CreateSuperAdmin")]
    public async Task<ActionResult<Guid>> CreateSuperAdmin()
    {
        var roleEntity = await _mainDatabase.Roles.GetSingleAsync(x => x.Name == UserRoles.SuperAdmin);
        if (roleEntity == null)
        {
            return BadRequest($"Role {nameof(UserRoles.SuperAdmin)} was not found");
        }

        var name = "su";
        var pass = Guid.NewGuid().ToString();
        
        var existingUserEntity = await _mainDatabase.Users.GetSingleAsync(x => x.Username == name);
        if (existingUserEntity != null)
        {
            return BadRequest("Super Admin user already exists");
        }

        var hashedPassword = _passwordHasher.HashPassword(pass);
        
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Empty,
            FirstName = "Super",
            LastName = "Admin",
            Username = name,
            PasswordHash = hashedPassword,
            CreatedDateTimeUtc = DateTime.UtcNow
        };
        
        _mainDatabase.Users.Create(userEntity);
        
        _mainDatabase.Users.AddUserToRole(userEntity, roleEntity);
        
        await _mainDatabase.CommitChangesAsync();

        return Ok(pass);
    }
}
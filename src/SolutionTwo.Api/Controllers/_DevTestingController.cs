using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Business.Common.PasswordHasher.Interfaces;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Tags("_DevTesting")] // for swagger
[Route("api/[controller]")]
[ApiController]
public class DevTestingController : ApiControllerBase
{
    private readonly IMainDatabase _mainDatabase;
    private readonly IPasswordHasher _passwordHasher;

    public DevTestingController(IMainDatabase mainDatabase, IPasswordHasher passwordHasher)
    {
        _mainDatabase = mainDatabase;
        _passwordHasher = passwordHasher;
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
using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Data.MainDatabase.Entities;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Tags("_DevTesting")] // for swagger
[Route("api/[controller]")]
[ApiController]
public class DevTestingController : ApiControllerBase
{
    private readonly IMainDatabase _mainDatabase;

    public DevTestingController(IMainDatabase mainDatabase)
    {
        _mainDatabase = mainDatabase;
    }
    
    [HttpPost("CreateUniqueFakeRole")]
    public async Task<ActionResult<Guid>> CreateUniqueFakeRole()
    {
        var uniqueRole = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = $"Fake-{Guid.NewGuid()}"
        };
        
        _mainDatabase.Roles.Create(uniqueRole);
        await _mainDatabase.CommitChangesAsync();

        return Ok(uniqueRole.Id);
    }
    
    [HttpPost("CreateTwoUniqueFakeRoles")]
    public async Task<ActionResult> CreateTwoUniqueFakeRoles()
    {
        var uniqueRole1 = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = $"Fake-{Guid.NewGuid()}"
        };
        
        var uniqueRole2 = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = $"Fake-{Guid.NewGuid()}"
        };
        
        _mainDatabase.Roles.Create(uniqueRole1);
        _mainDatabase.Roles.Create(uniqueRole2);
        await _mainDatabase.CommitChangesAsync();

        return Ok(new { uniqueRole1Id = uniqueRole1.Id, uniqueRole2d = uniqueRole2.Id });
    }

    [HttpPost("AddUniqueFakeRoleToUser")]
    public async Task<ActionResult> AddUniqueFakeRoleToUser(AddUniqueFakeRoleToUserRequest request)
    {
        // with 'Update' method (without 'withTracking') will be additional unnecessary updates fot user and role entries  
        var user = await _mainDatabase.Users.GetByIdAsync(request.UserId, withTracking: true);
        var role = await _mainDatabase.Roles.GetByIdAsync(request.RoleId, withTracking: true);

        user!.Roles.Add(role!);

        await _mainDatabase.CommitChangesAsync();

        return Ok();
    }
    
    [HttpPost("AddUniqueFakeRoleToUser2")]
    public async Task<ActionResult> AddUniqueFakeRoleToUser2(AddUniqueFakeRoleToUserRequest request)
    {
        var user = await _mainDatabase.Users.GetByIdAsync(request.UserId);
        var role = await _mainDatabase.Roles.GetByIdAsync(request.RoleId);

        _mainDatabase.Roles.AddRoleToUser(role!, user!);

        await _mainDatabase.CommitChangesAsync();

        return Ok();
    }
}

public class AddUniqueFakeRoleToUserRequest
{
    public Guid RoleId { get; set; }

    public Guid UserId { get; set; }
}
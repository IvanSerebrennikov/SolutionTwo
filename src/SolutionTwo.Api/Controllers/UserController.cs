using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Business.Core.Models.User.Incoming;
using SolutionTwo.Business.Core.Models.User.Outgoing;
using SolutionTwo.Business.Core.Services.Interfaces;
using SolutionTwo.Business.Identity.Services.Interfaces;

namespace SolutionTwo.Api.Controllers;

[SolutionTwoAuthorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ApiAuthorizedControllerBase
{
    private readonly IUserService _userService;
    private readonly IIdentityService _identityService;

    public UserController(IUserService userService, IIdentityService identityService)
    {
        _userService = userService;
        _identityService = identityService;
    }
    
    [HttpGet("me")]
    public async Task<ActionResult<UserWithRolesModel>> GetMe()
    {
        var userModel = await _userService.GetUserWithRolesByIdAsync(CurrentUserId);

        if (userModel == null)
            return NotFound();
        
        return Ok(userModel);
    }
    
    [SolutionTwoAuthorize(UserRoles.TenantAdmin)]
    [HttpPost]
    public async Task<ActionResult> CreateTenantUser(CreateUserModel createUserModel)
    {
        var serviceResult = await _userService.CreateTenantUserAsync(createUserModel);
        
        if (!serviceResult.IsSucceeded || serviceResult.Data == null)
            return BadRequest(serviceResult);
        
        var userModel = serviceResult.Data;

        return CreatedAtAction(nameof(GetById), new { id = userModel.Id }, userModel);
    }

    [SolutionTwoAuthorize(UserRoles.SuperAdmin, UserRoles.TenantAdmin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserWithRolesModel>>> GetAll()
    {
        var userModels = await _userService.GetAllUsersWithRolesAsync();

        return Ok(userModels);
    }

    [SolutionTwoAuthorize(UserRoles.SuperAdmin, UserRoles.TenantAdmin)]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserWithRolesModel>> GetById(Guid id)
    {
        var userModel = await _userService.GetUserWithRolesByIdAsync(id);

        if (userModel == null)
            return NotFound();
        
        return Ok(userModel);
    }

    [SolutionTwoAuthorize(UserRoles.SuperAdmin, UserRoles.TenantAdmin)]
    [HttpDelete]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);

        if (!result.IsSucceeded)
            return BadRequest(result.Message);

        await _identityService.ResetUserAccessAsync(id);

        return Ok();
    }
    
    [SolutionTwoAuthorize(UserRoles.SuperAdmin, UserRoles.TenantAdmin)]
    [HttpPost("{id}/logout")]
    public async Task<ActionResult> LogOutUser(Guid id)
    {
        var userExists = await _userService.UserExistsAsync(id);

        if (!userExists)
            return BadRequest("User was not found");
        
        await _identityService.ResetUserAccessAsync(id);
        
        return Ok();
    }
}
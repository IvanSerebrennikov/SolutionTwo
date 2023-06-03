using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Business.Core.Models.User.Incoming;
using SolutionTwo.Business.Core.Models.User.Outgoing;
using SolutionTwo.Business.Core.Services.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ApiAuthorizedControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [RoleBasedAuthorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserWithRolesModel>> GetMe()
    {
        var userModel = await _userService.GetUserWithRolesAsync(CurrentUserUsername);

        if (userModel == null)
            return NotFound();
        
        return Ok(userModel);
    }

    [RoleBasedAuthorize(UserRoles.SuperAdmin, UserRoles.TenantAdmin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserWithRolesModel>>> GetAll()
    {
        var userModels = await _userService.GetAllUsersWithRolesAsync();

        return Ok(userModels);
    }

    [RoleBasedAuthorize(UserRoles.SuperAdmin, UserRoles.TenantAdmin)]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserWithRolesModel>> GetById(Guid id)
    {
        var userModel = await _userService.GetUserWithRolesByIdAsync(id);

        if (userModel == null)
            return NotFound();
        
        return Ok(userModel);
    }

    [RoleBasedAuthorize(UserRoles.SuperAdmin, UserRoles.TenantAdmin)]
    [HttpPost]
    public async Task<ActionResult> CreateUser(CreateUserModel createUserModel)
    {
        if (string.IsNullOrEmpty(createUserModel.FirstName) ||
            string.IsNullOrEmpty(createUserModel.LastName) ||
            string.IsNullOrEmpty(createUserModel.Username) ||
            string.IsNullOrEmpty(createUserModel.Password))
            return BadRequest("Passed data is invalid. All properties are required.");

        var userModel = await _userService.CreateUserAsync(createUserModel);

        return CreatedAtAction(nameof(GetById), new { id = userModel.Id }, userModel);
    }

    [RoleBasedAuthorize(UserRoles.SuperAdmin, UserRoles.TenantAdmin)]
    [HttpDelete]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);

        if (!result.IsSucceeded)
        {
            return BadRequest(result.Message);
        }

        return Ok();
    }
}
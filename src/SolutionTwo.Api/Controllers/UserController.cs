using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Core.Constants;
using SolutionTwo.Business.Core.Models.User.Incoming;
using SolutionTwo.Business.Core.Models.User.Outgoing;
using SolutionTwo.Business.Core.Services.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ApiControllerBase
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
        var username = GetUsernameFromClaims();
        if (string.IsNullOrEmpty(username))
            return BadRequest("Name claim was not found");

        var userModel = await _userService.GetUserWithRolesAsync(username);

        if (userModel == null)
            return NotFound();
        
        return Ok(userModel);
    }

    [RoleBasedAuthorize(UserRoles.SuperAdmin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserWithRolesModel>>> GetAll()
    {
        var userModels = await _userService.GetAllUsersWithRolesAsync();

        return Ok(userModels);
    }

    [RoleBasedAuthorize(UserRoles.SuperAdmin)]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserWithRolesModel>> GetById(Guid id)
    {
        var userModel = await _userService.GetUserWithRolesByIdAsync(id);

        if (userModel == null)
            return NotFound();
        
        return Ok(userModel);
    }

    [RoleBasedAuthorize(UserRoles.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult> AddUser(CreateUserModel createUserModel)
    {
        if (string.IsNullOrEmpty(createUserModel.FirstName) ||
            string.IsNullOrEmpty(createUserModel.LastName) ||
            string.IsNullOrEmpty(createUserModel.Username) ||
            string.IsNullOrEmpty(createUserModel.Password))
            return BadRequest("Passed data is invalid. All properties are required.");

        var userModel = await _userService.AddUserAsync(createUserModel);

        return CreatedAtAction(nameof(GetById), new { id = userModel.Id }, userModel);
    }

    [RoleBasedAuthorize(UserRoles.SuperAdmin)]
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
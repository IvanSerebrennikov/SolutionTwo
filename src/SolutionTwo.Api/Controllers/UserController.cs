using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Domain.Constants;
using SolutionTwo.Domain.Models.User.Incoming;
using SolutionTwo.Domain.Models.User.Outgoing;
using SolutionTwo.Domain.Services.Interfaces;

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

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserWithRolesModel>> GetMe()
    {
        var username = GetUsernameFromClaims();
        if (string.IsNullOrEmpty(username))
            return BadRequest("Name claim was not found");
        
        var userModel = await _userService.GetUserWithRolesAsync(username);

        if (userModel != null)
        {
            return Ok(userModel);
        }
        else
        {
            return NotFound();
        }
    }
    
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserWithRolesModel>>> GetAll()
    {
        var userModels = await _userService.GetAllUsersWithRolesAsync();

        return Ok(userModels);
    }
    
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserWithRolesModel>> GetById(Guid id)
    {
        var userModel = await _userService.GetUserWithRolesByIdAsync(id);

        if (userModel != null)
        {
            return Ok(userModel);
        }
        else
        {
            return NotFound();
        }
    }

    [Authorize(Roles = UserRoles.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult> AddUser(CreateUserModel createUserModel)
    {
        if (string.IsNullOrEmpty(createUserModel.FirstName) ||
            string.IsNullOrEmpty(createUserModel.LastName) ||
            string.IsNullOrEmpty(createUserModel.Username) ||
            string.IsNullOrEmpty(createUserModel.Password))
        {
            return BadRequest("Passed data is invalid. All properties are required.");
        }
        
        var userModel = await _userService.AddUserAsync(createUserModel);
        
        return CreatedAtAction(nameof(GetById), new { id = userModel.Id }, userModel);
    }
}
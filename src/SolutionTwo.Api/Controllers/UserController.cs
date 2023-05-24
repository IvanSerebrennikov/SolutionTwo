using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Models;
using SolutionTwo.Domain.Constants;
using SolutionTwo.Domain.Models.User.Incoming;
using SolutionTwo.Domain.Models.User.Outgoing;
using SolutionTwo.Domain.Services.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
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
        if (!createUserModel.IsValid(out string errorMessage))
        {
            var errorResponse = new ErrorResponse(errorMessage);
            return BadRequest(errorResponse);
        }
        
        var userModel = await _userService.AddUserAsync(createUserModel);
        
        return CreatedAtAction(nameof(GetById), new { id = userModel.Id }, userModel);
    }
}
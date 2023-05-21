using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Models;
using SolutionTwo.Domain.Models.User;
using SolutionTwo.Domain.Services.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IUserService _userService;

    public UserController(ILogger<WeatherForecastController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserModel>>> GetAll()
    {
        var userModels = await _userService.GetAllUsersAsync();

        return Ok(userModels);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UserModel>> GetById(Guid id)
    {
        var userModel = await _userService.GetUserAsync(id);

        if (userModel != null)
        {
            return Ok(userModel);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult> AddUser(UserCreationModel userCreationModel)
    {
        if (!userCreationModel.IsValid(out string errorMessage))
        {
            var errorResponse = new ErrorResponse(errorMessage);
            return BadRequest(errorResponse);
        }
        
        var userModel = await _userService.AddUserAsync(userCreationModel);
        
        return CreatedAtAction(nameof(GetById), new { id = userModel.Id }, userModel);
    }
}
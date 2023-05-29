using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Api.Models;
using SolutionTwo.Business.Core.Models.User.Incoming;
using SolutionTwo.Business.Core.Services.Interfaces;
using SolutionTwo.Business.Identity.Models.Auth.Outgoing;
using SolutionTwo.Business.Identity.Services.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<TokensPairModel>> Auth(UserCredentialsModel userCredentials)
    {
        if (string.IsNullOrWhiteSpace(userCredentials.Username) || string.IsNullOrWhiteSpace(userCredentials.Password))
            return BadRequest("Credentials can't be empty");

        var userServiceResult = await _userService.GetUserWithRolesByCredentialsAsync(userCredentials);
        
        if (!userServiceResult.IsSucceeded || userServiceResult.Data == null)
            return BadRequest(userServiceResult);
        
        var authServiceResult = await _authService.CreateTokensPairAsync(userServiceResult.Data.Id);

        if (!authServiceResult.IsSucceeded || authServiceResult.Data == null)
            return BadRequest(authServiceResult);

        var response = new AuthResponse(authServiceResult.Data, userServiceResult.Data.FirstName,
            userServiceResult.Data.LastName);
        
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokensPairModel>> RefreshToken([FromBody] string refreshTokenValue)
    {
        if (string.IsNullOrEmpty(refreshTokenValue) || !Guid.TryParse(refreshTokenValue, out var refreshTokenId))
            return BadRequest("Invalid Refresh token");

        var serviceResult = await _authService.RefreshTokensPairAsync(refreshTokenId);

        if (!serviceResult.IsSucceeded)
            return BadRequest(serviceResult);
        
        return Ok(serviceResult.Data);
    }
}
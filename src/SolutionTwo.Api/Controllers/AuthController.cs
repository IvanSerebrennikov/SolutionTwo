using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Models;
using SolutionTwo.Api.Models.Auth;
using SolutionTwo.Domain.Services.Interfaces;
using SolutionTwo.Identity.PasswordProcessing.Interfaces;
using SolutionTwo.Identity.TokenProvider.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenProvider _tokenProvider;
    private readonly IPasswordProcessor _passwordProcessor;

    public AuthController(IUserService userService, ITokenProvider tokenProvider, IPasswordProcessor passwordProcessor)
    {
        _userService = userService;
        _tokenProvider = tokenProvider;
        _passwordProcessor = passwordProcessor;
    }

    [HttpPost]
    public async Task<ActionResult<AuthResponse>> Auth(AuthRequest authRequest)
    {
        const string invalidCredentialsMessage =
            $"Invalid {nameof(authRequest.Username)} or {nameof(authRequest.Password)}";
        const string userNotFoundMessage =
            $"User with provided {nameof(authRequest.Username)} and {nameof(authRequest.Password)} was not found";

        if (!authRequest.IsValid())
            return BadRequest(new ErrorResponse(invalidCredentialsMessage));

        var user = await _userService.GetUserWithRolesAsync(authRequest.Username!);
        if (user == null)
            return NotFound(new ErrorResponse(userNotFoundMessage));

        if (!_passwordProcessor.VerifyHashedPassword(user.UserData.Id, user.PasswordHash,
                authRequest.Password!))
            return NotFound(new ErrorResponse(userNotFoundMessage));

        var roles = string.Join(", ", user.Roles);
        var authToken =
            _tokenProvider.GenerateAuthToken((ClaimTypes.Name, authRequest.Username!), (ClaimTypes.Role, roles));
        
        // _userService.CreateRefreshTokenForUser(user or userId)

        var authResponse = new AuthResponse(authToken, "REFRESH");
        
        return Ok(authResponse);
    }
}
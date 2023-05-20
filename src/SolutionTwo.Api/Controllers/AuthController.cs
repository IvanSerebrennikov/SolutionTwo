using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Models.Auth;
using SolutionTwo.Domain.Services.Interfaces;
using SolutionTwo.Identity.TokenProvider.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenProvider _tokenProvider;

    public AuthController(IUserService userService, ITokenProvider tokenProvider)
    {
        _userService = userService;
        _tokenProvider = tokenProvider;
    }

    [HttpPost]
    public async Task<ActionResult<AuthResponse>> Auth(AuthRequest authRequest)
    {
        const string invalidCredentialsMessage =
            $"Invalid {nameof(authRequest.Username)} or {nameof(authRequest.Password)}";
        const string userNotFoundMessage =
            $"User with provided {nameof(authRequest.Username)} and {nameof(authRequest.Password)} not found";

        if (!authRequest.IsValid())
            return BadRequest(invalidCredentialsMessage);
        
        // _userService.GetUserWithRolesAsync(by username, include roles during repo call)
        
        // _passwordProcessor.VerifyHashedPassword(...)
        
        var authToken =
            _tokenProvider.GenerateAuthToken((ClaimTypes.Name, authRequest.Username!), (ClaimTypes.Role, "Admin"));
        
        // _userService.CreateRefreshTokenForUser(user or userId)

        var authResponse = new AuthResponse(authToken, "REFRESH");
        
        return Ok(authResponse);
    }
}
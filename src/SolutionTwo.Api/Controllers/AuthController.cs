using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Models;
using SolutionTwo.Api.Models.Auth;
using SolutionTwo.Domain.Models.User.Outgoing;
using SolutionTwo.Domain.Services.Interfaces;
using SolutionTwo.Identity.PasswordManaging.Interfaces;
using SolutionTwo.Identity.TokenManaging.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ITokenManager _tokenManager;
    private readonly IPasswordManager _passwordManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService, 
        ITokenManager tokenManager, 
        IPasswordManager passwordManager, 
        IUserService userService, 
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _tokenManager = tokenManager;
        _passwordManager = passwordManager;
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<AuthResponse>> Auth(AuthRequest authRequest)
    {
        if (!authRequest.IsValid())
            return BadRequest(
                new ErrorResponse($"Invalid {nameof(authRequest.Username)} or {nameof(authRequest.Password)}"));

        const string userNotFoundMessage =
            $"User with provided {nameof(authRequest.Username)} and {nameof(authRequest.Password)} was not found";

        var user = await _userService.GetUserWithRolesAsync(authRequest.Username!);
        if (user == null)
            return NotFound(new ErrorResponse(userNotFoundMessage));

        if (!_passwordManager.VerifyHashedPassword(user.PasswordHash,
                authRequest.Password!))
            return NotFound(new ErrorResponse(userNotFoundMessage));

        var authToken = CreateAuthToken(user.UserData, out var authTokenId);
        
        var refreshTokenId = await _authService.CreateRefreshTokenForUserAsync(user.UserData.Id, authTokenId);
        
        var authResponse = new AuthResponse(authToken, refreshTokenId.ToString());
        
        return Ok(authResponse);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody]string refreshTokenValue)
    {
        if (string.IsNullOrEmpty(refreshTokenValue) || !Guid.TryParse(refreshTokenValue, out var refreshTokenId))
            return BadRequest("Invalid Refresh token");

        var refreshToken = await _authService.GetRefreshTokenAsync(refreshTokenId);

        if (refreshToken == null)
            return BadRequest("Provided Refresh token was not found");

        if (refreshToken.IsExpired)
            return BadRequest("Provided Refresh token expired");

        if (refreshToken.IsRevoked)
            return BadRequest("Provided Refresh token was revoked");

        if (refreshToken.IsUsed)
        {
            _logger.LogWarning(
                $"Someone is trying to refresh already used token. RefreshTokenId: {refreshToken.Id}, UserId: {refreshToken.UserId}.");
            await _authService.RevokeProvidedAndAllActiveRefreshTokensForUserAsync(refreshToken.Id, refreshToken.UserId);
            return BadRequest("Provided Refresh token already used");
        }
        
        var user = await _userService.GetUserWithRolesByIdAsync(refreshToken.UserId);
        if (user == null)
            return BadRequest("Associated User was not found");
        
        var authToken = CreateAuthToken(user, out var authTokenId);

        var newRefreshTokenId =
            await _authService.MarkRefreshTokenAsUsedAndCreateNewOneAsync(refreshToken.Id, authTokenId);

        var authResponse = new AuthResponse(authToken, newRefreshTokenId.ToString());
        
        return Ok(authResponse);
    }
    
    private string CreateAuthToken(UserWithRolesModel user, out Guid authTokenId)
    {
        var claims = user.Roles.Select(x => (ClaimTypes.Role, x)).ToList();
        claims.Add((ClaimTypes.Name, user.Username));
        var authToken =
            _tokenManager.GenerateAuthToken(claims, out authTokenId);
        return authToken;
    }
}
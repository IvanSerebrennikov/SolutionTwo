using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Identity.Models.Auth.Incoming;
using SolutionTwo.Business.Identity.Models.Auth.Outgoing;
using SolutionTwo.Business.Identity.Services.Interfaces;

namespace SolutionTwo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    public async Task<ActionResult<AuthResult>> Auth(UserCredentialsModel userCredentials)
    {
        if (string.IsNullOrWhiteSpace(userCredentials.Username) || string.IsNullOrWhiteSpace(userCredentials.Password))
            return BadRequest("Credentials can't be empty");
        
        var authServiceResult = await _authService.ValidateCredentialsAndCreateTokensPairAsync(userCredentials);

        if (!authServiceResult.IsSucceeded || authServiceResult.Data == null)
            return BadRequest(authServiceResult);

        return Ok(authServiceResult.Data);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokensPairModel>> RefreshToken([FromBody] string refreshTokenValue)
    {
        if (string.IsNullOrEmpty(refreshTokenValue))
            return BadRequest("Invalid Refresh token");

        var serviceResult = await _authService.RefreshTokensPairAsync(refreshTokenValue);

        if (!serviceResult.IsSucceeded)
            return BadRequest(serviceResult);
        
        return Ok(serviceResult.Data);
    }
}
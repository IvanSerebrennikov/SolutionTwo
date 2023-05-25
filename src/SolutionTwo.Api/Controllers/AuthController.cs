using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Api.Models;
using SolutionTwo.Domain.Models.Auth.Incoming;
using SolutionTwo.Domain.Models.Auth.Outgoing;
using SolutionTwo.Domain.Services.Interfaces;

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
    public async Task<ActionResult<TokensPairModel>> Auth(UserCredentialsModel userCredentials)
    {
        if (string.IsNullOrWhiteSpace(userCredentials.Username) || string.IsNullOrWhiteSpace(userCredentials.Password))
            return BadRequest("Invalid credentials");

        var serviceResult = await _authService.CreateTokensPairAsync(userCredentials);

        if (serviceResult.IsSucceeded)
        {
            return Ok(serviceResult.Data);
        }
        else
        {
            return BadRequest(serviceResult.Message);
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokensPairModel>> RefreshToken([FromBody]string refreshTokenValue)
    {
        if (string.IsNullOrEmpty(refreshTokenValue) || !Guid.TryParse(refreshTokenValue, out var refreshTokenId))
            return BadRequest("Invalid Refresh token");

        var serviceResult = await _authService.RefreshTokensPairAsync(refreshTokenId);

        if (serviceResult.IsSucceeded)
        {
            return Ok(serviceResult.Data);
        }
        else
        {
            return BadRequest(serviceResult.Message);
        }
    }
}
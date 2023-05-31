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
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost]
    public async Task<ActionResult<AuthResult>> Auth(UserCredentialsModel userCredentials)
    {
        if (string.IsNullOrWhiteSpace(userCredentials.Username) || string.IsNullOrWhiteSpace(userCredentials.Password))
            return BadRequest("Credentials can't be empty");
        
        var serviceResult = await _identityService.ValidateCredentialsAndCreateTokensPairAsync(userCredentials);

        if (!serviceResult.IsSucceeded || serviceResult.Data == null)
            return BadRequest(serviceResult);

        return Ok(serviceResult.Data);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokensPairModel>> RefreshToken([FromBody] string refreshTokenValue)
    {
        if (string.IsNullOrEmpty(refreshTokenValue))
            return BadRequest("Invalid Refresh token");

        var serviceResult = await _identityService.RefreshTokensPairAsync(refreshTokenValue);

        if (!serviceResult.IsSucceeded)
            return BadRequest(serviceResult);
        
        return Ok(serviceResult.Data);
    }
}
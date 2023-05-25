using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Models;

namespace SolutionTwo.Api.Controllers.Base;

public class ApiControllerBase : ControllerBase
{
    protected BadRequestObjectResult BadRequest(string? errorMessage)
    {
        return BadRequest(new ErrorResponse(errorMessage));
    }

    protected string? GetUsernameFromClaims()
    {
        return GetClaimValue(ClaimTypes.Name);
    }

    private string? GetClaimValue(string claimType)
    {
        return HttpContext.User.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
    }
}
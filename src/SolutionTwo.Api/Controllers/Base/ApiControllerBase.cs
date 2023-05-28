using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Models;
using SolutionTwo.Business.Common.Models;

namespace SolutionTwo.Api.Controllers.Base;

public class ApiControllerBase : ControllerBase
{
    protected BadRequestObjectResult BadRequest(string? errorMessage, Guid? errorId = null)
    {
        return BadRequest(new ErrorResponse(errorMessage, errorId));
    }
    
    protected BadRequestObjectResult BadRequest(IServiceResult serviceResult)
    {
        return BadRequest(new ErrorResponse(serviceResult.Message, serviceResult.TraceId));
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
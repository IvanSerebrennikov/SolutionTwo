using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Models;
using SolutionTwo.Business.Common.Models;

namespace SolutionTwo.Api.Controllers.Base;

public class ApiControllerBase : ControllerBase
{
    protected BadRequestObjectResult BadRequest(string? errorMessage)
    {
        var traceId = HttpContext.TraceIdentifier;
        return BadRequest(new ErrorResponse(errorMessage, traceId));
    }

    protected BadRequestObjectResult BadRequest(IServiceResult serviceResult)
    {
        return BadRequest(serviceResult.Message);
    }
}
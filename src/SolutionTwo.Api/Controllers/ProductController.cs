using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Common.Constants;

namespace SolutionTwo.Api.Controllers;

[SolutionTwoAuthorize]
[Route("api/[controller]")]
[ApiController]
public class ProductController : ApiAuthorizedControllerBase
{
    [SolutionTwoAuthorize(UserRoles.TenantAdmin, UserRoles.TenantUser)]
    [HttpPost("{id}/hold")]
    public async Task<ActionResult> HoldProduct(Guid id)
    {
        return Ok();
    }
    
    [SolutionTwoAuthorize(UserRoles.TenantAdmin, UserRoles.TenantUser)]
    [HttpPost("{id}/release")]
    public async Task<ActionResult> ReleaseProduct(Guid id)
    {
        return Ok();
    }
    
    [SolutionTwoAuthorize(UserRoles.TenantAdmin)]
    [HttpPost]
    public async Task<ActionResult> CreateProduct()
    {
        return Ok();
    }
}
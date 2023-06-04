using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Attributes;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Common.Constants;
using SolutionTwo.Business.MultiTenancy.Models.Tenant.Incoming;
using SolutionTwo.Business.MultiTenancy.Models.Tenant.Outgoing;
using SolutionTwo.Business.MultiTenancy.Services.Interfaces;

namespace SolutionTwo.Api.Controllers;

[RoleBasedAuthorize(UserRoles.SuperAdmin)]
[Route("api/[controller]")]
[ApiController]
public class TenantController : ApiControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantModel>>> GetAll()
    {
        var tenantModels = await _tenantService.GetAllTenantsAsync();

        return Ok(tenantModels);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TenantModel>> GetById(Guid id)
    {
        var tenantModel = await _tenantService.GetTenantByIdAsync(id);

        if (tenantModel == null)
            return NotFound();
        
        return Ok(tenantModel);
    }

    [HttpPost]
    public async Task<ActionResult> CreateTenant(CreateTenantModel createTenantModel)
    {
        if (string.IsNullOrEmpty(createTenantModel.TenantName) ||
            string.IsNullOrEmpty(createTenantModel.AdminFirstName) ||
            string.IsNullOrEmpty(createTenantModel.AdminLastName) ||
            string.IsNullOrEmpty(createTenantModel.AdminUsername) ||
            string.IsNullOrEmpty(createTenantModel.AdminPassword))
            return BadRequest("Passed data is invalid. All properties are required.");

        var serviceResult =  await _tenantService.CreateTenantAsync(createTenantModel);

        if (!serviceResult.IsSucceeded || serviceResult.Data == null)
            return BadRequest(serviceResult);
        
        var tenantModel = serviceResult.Data;

        return CreatedAtAction(nameof(GetById), new { id = tenantModel.Id }, tenantModel);
    }
}
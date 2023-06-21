using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Business.Core.Models.Product.Outgoing;
using SolutionTwo.Common.MaintenanceStatusAccessor.Enums;
using SolutionTwo.Common.MaintenanceStatusAccessor.Interfaces;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Api.Controllers.Internal;

[Tags("_ApplicationManaging")] // for swagger
[Route("api/[controller]")]
[ApiController]
public class ApplicationManagingController : ApiControllerBase
{
    private readonly IMainDatabase _mainDatabase;
    private readonly IMaintenanceStatusSetter _maintenanceStatusSetter;
    private readonly IMaintenanceStatusGetter _maintenanceStatusGetter;

    public ApplicationManagingController(
        IMaintenanceStatusSetter maintenanceStatusSetter, 
        IMaintenanceStatusGetter maintenanceStatusGetter, 
        IMainDatabase mainDatabase)
    {
        _maintenanceStatusSetter = maintenanceStatusSetter;
        _maintenanceStatusGetter = maintenanceStatusGetter;
        _mainDatabase = mainDatabase;
    }

    [HttpPost("[action]")]
    public ActionResult SetMaintenanceStatus([FromBody]MaintenanceStatus maintenanceStatus)
    {
        if (!Enum.IsDefined(typeof(MaintenanceStatus), maintenanceStatus))
        {
            return BadRequest("Invalid maintenance status was provided");
        }

        _maintenanceStatusSetter.SetMaintenanceStatus(maintenanceStatus);
        
        var newMaintenanceStatusString = _maintenanceStatusGetter.MaintenanceStatus.HasValue
            ? _maintenanceStatusGetter.MaintenanceStatus.Value.ToString()
            : "-";
        
        return Ok($"New maintenance status: {newMaintenanceStatusString}");
    }
    
    [HttpPost("[action]")]
    public ActionResult ResetMaintenanceStatus()
    {
        _maintenanceStatusSetter.SetMaintenanceStatus(null);
        
        var newMaintenanceStatusString = _maintenanceStatusGetter.MaintenanceStatus.HasValue
            ? _maintenanceStatusGetter.MaintenanceStatus.Value.ToString()
            : "-";
        
        return Ok($"New maintenance status: {newMaintenanceStatusString}");
    }

    [HttpGet("[action]")]
    public async Task<ActionResult<IEnumerable<ProductWithActiveUsagesModel>>> CheckProductsThatAreCurrentlyInActiveUse()
    {
        var productEntities = await _mainDatabase.Products.GetAsync(x => x.CurrentActiveUsagesCount > 0,
            include: x => x.ProductUsages.Where(u => u.ReleasedDateTimeUtc == null));
        var productModels = productEntities.Select(x => new ProductWithActiveUsagesModel(x)).ToList();
        
        return Ok(productModels);
    }
}
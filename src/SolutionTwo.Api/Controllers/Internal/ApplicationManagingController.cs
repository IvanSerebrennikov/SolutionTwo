using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Common.MaintenanceStatusAccessor.Enums;
using SolutionTwo.Common.MaintenanceStatusAccessor.Interfaces;

namespace SolutionTwo.Api.Controllers.Internal;

[Tags("_ApplicationManaging")] // for swagger
[Route("api/application-managing")]
[ApiController]
public class ApplicationManagingController : ApiControllerBase
{
    private readonly IMaintenanceStatusSetter _maintenanceStatusSetter;
    private readonly IMaintenanceStatusGetter _maintenanceStatusGetter;

    public ApplicationManagingController(
        IMaintenanceStatusSetter maintenanceStatusSetter, 
        IMaintenanceStatusGetter maintenanceStatusGetter)
    {
        _maintenanceStatusSetter = maintenanceStatusSetter;
        _maintenanceStatusGetter = maintenanceStatusGetter;
    }

    [HttpPost("set-maintenance-status")]
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
    
    [HttpPost("reset-maintenance-status")]
    public ActionResult ResetMaintenanceStatus()
    {
        _maintenanceStatusSetter.SetMaintenanceStatus(null);
        
        var newMaintenanceStatusString = _maintenanceStatusGetter.MaintenanceStatus.HasValue
            ? _maintenanceStatusGetter.MaintenanceStatus.Value.ToString()
            : "-";
        
        return Ok($"New maintenance status: {newMaintenanceStatusString}");
    }

    [HttpGet("check-products-that-are-currently-in-active-use")]
    public ActionResult CheckProductsThatAreCurrentlyInActiveUse()
    {
        return Ok();
    }
}
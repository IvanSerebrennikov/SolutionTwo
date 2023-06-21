using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Common.MaintenanceStatusAccessor.Enums;
using SolutionTwo.Common.MaintenanceStatusAccessor.Interfaces;

namespace SolutionTwo.Api.Controllers.Internal;

[Tags("_ApplicationManaging")] // for swagger
[Route("api/[controller]")]
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
    public ActionResult CheckProductsThatAreCurrentlyInActiveUse()
    {
        return Ok();
    }
}
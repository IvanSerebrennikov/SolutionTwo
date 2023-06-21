using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Api.Controllers.Internal;

[Tags("__DevTesting")] // for swagger
[Route("api/dev-testing")]
[ApiController]
public class DevTestingController : ApiControllerBase
{
    private readonly IMainDatabase _mainDatabase;

    public DevTestingController(IMainDatabase mainDatabase)
    {
        _mainDatabase = mainDatabase;
    }

    [HttpGet("test-ok")]
    public ActionResult TestOk()
    {
        return Ok();
    }
    
    [HttpGet("test-error")]
    public ActionResult TestError()
    {
        return BadRequest("test error message");
    }
}
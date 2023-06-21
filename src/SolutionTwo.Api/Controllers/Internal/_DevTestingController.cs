using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Controllers.Base;
using SolutionTwo.Data.MainDatabase.UnitOfWork.Interfaces;

namespace SolutionTwo.Api.Controllers.Internal;

[Tags("__DevTesting")] // for swagger
[Route("api/[controller]")]
[ApiController]
public class DevTestingController : ApiControllerBase
{
    private readonly IMainDatabase _mainDatabase;

    public DevTestingController(IMainDatabase mainDatabase)
    {
        _mainDatabase = mainDatabase;
    }

    [HttpGet("TestOk")]
    public ActionResult TestOk()
    {
        return Ok();
    }
    
    [HttpGet("TestError")]
    public ActionResult TestError()
    {
        return BadRequest("test error message");
    }
}
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

    [HttpGet("[action]")]
    public ActionResult GetOk()
    {
        return Ok();
    }
    
    [HttpGet("[action]/{testMessage}")]
    public ActionResult GetMessageUppercase(string testMessage)
    {
        return Ok($"Message: {testMessage.ToUpper()}");
    }
    
    [HttpGet("[action]")]
    public ActionResult GetError()
    {
        return BadRequest("test error message");
    }
}
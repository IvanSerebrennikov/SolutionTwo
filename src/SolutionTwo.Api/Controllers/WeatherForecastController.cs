using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolutionTwo.Api.Models;
using SolutionTwo.Domain.Constants;

namespace SolutionTwo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [Authorize]
    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
    
    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("{amount}")]
    public IEnumerable<WeatherForecast> GetMore(int amount)
    {
        return Enumerable.Range(1, amount).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
    
    [Authorize(Roles = $"{UserRoles.SuperAdmin}, {UserRoles.Admin}")]
    [HttpGet("doubled/{amount}")]
    public IEnumerable<WeatherForecast> GetDoubled(int amount)
    {
        return Enumerable.Range(1, amount * 2).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}
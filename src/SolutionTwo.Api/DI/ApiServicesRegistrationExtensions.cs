using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using SolutionTwo.Api.Models;

namespace SolutionTwo.Api.DI;

public static class ApiServicesRegistrationExtensions
{
    public static void AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.Configure<ApiBehaviorOptions>(o =>
        {
            o.InvalidModelStateResponseFactory = actionContext =>
            {
                var traceId = actionContext.HttpContext.TraceIdentifier;
                var errorMessage = actionContext.ModelState.Values.SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage);
                return new BadRequestObjectResult(new ErrorResponse(errorMessage.ToArray(), traceId));
            };
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // https://aka.ms/aspnetcore/swashbuckle
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Bearer Authentication with JWT",
                Type = SecuritySchemeType.Http
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
        });
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;
using SolutionTwo.Api.Configuration;
using SolutionTwo.Api.Helpers;
using SolutionTwo.Api.Models;
using SolutionTwo.Common.Extensions;

namespace SolutionTwo.Api.DI;

public static class ApiServicesRegistrationExtensions
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers
        services.AddControllers(options => 
        {
            options.Conventions.Add(new RouteTokenTransformerConvention(new DashedLowercaseParameterTransformer()));
        });
        
        // Model state error (DataAnnotations validation error) response
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
        
        // Open Api (Swagger)
        services.ConfigureSwagger();
        
        // Other
        var useHardCodedIdentity =
            configuration.GetValue<bool?>($"{nameof(HardCodedIdentityConfiguration)}:UseHardCodedIdentity");
        var hardCodedIdentityConfiguration =
            configuration.GetSection<HardCodedIdentityConfiguration>(withValidation: useHardCodedIdentity == true);
        
        services.AddSingleton(hardCodedIdentityConfiguration);
        
        var basicAuthenticationConfiguration = configuration.GetSection<BasicAuthenticationConfiguration>();
        
        services.AddSingleton(basicAuthenticationConfiguration);
    }

    private static void ConfigureSwagger(this IServiceCollection services)
    {
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
            
            options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
            {
                Scheme = "Basic",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Basic Authentication with Base64(username:password)",
                Type = SecuritySchemeType.Http
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Basic",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
        });
    }
}
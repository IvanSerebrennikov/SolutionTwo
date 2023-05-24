using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SolutionTwo.Api.Middlewares;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.Configuration;
using SolutionTwo.Data.Context;
using SolutionTwo.Data.DI;
using SolutionTwo.Domain.DI;
using SolutionTwo.Identity.Configuration;
using SolutionTwo.Identity.DI;
using SolutionTwo.Identity.TokenManaging.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// Api DI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
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

// Identity DI
var identityConfiguration = builder.Configuration.GetSection<IdentityConfiguration>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = identityConfiguration.JwtIssuer,
        ValidAudience = identityConfiguration.JwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(identityConfiguration.JwtKey!))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = ctx =>
        {
            var id = ctx.SecurityToken.Id;
            var tokenManager = ctx.HttpContext.RequestServices.GetRequiredService<ITokenManager>();

            if (!Guid.TryParse(id, out var authTokenId) || tokenManager.IsTokenDeactivated(authTokenId))
            {
                ctx.Fail("Access was revoked.");
            }
            
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddIdentityServices();

// Domain DI
builder.Services.AddDomainServices();

// Data DI
var connectionStrings = builder.Configuration.GetSection<ConnectionStrings>();
builder.Services.AddDbContext<MainDatabaseContext>(o =>
    {
        o.UseSqlServer(connectionStrings.MainDatabaseConnection!);
        
        // Make sure that "Microsoft.EntityFrameworkCore" category is set to "None" 
        // for all providers except "Debug"
        o.EnableSensitiveDataLogging();  
    }
);
builder.Services.AddDataServices();

// Build WebApp
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalErrorHandlingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
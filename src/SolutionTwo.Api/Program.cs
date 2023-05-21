using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SolutionTwo.Api.Middlewares;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.Configuration;
using SolutionTwo.Data.Context;
using SolutionTwo.Data.Repositories;
using SolutionTwo.Data.Repositories.Interfaces;
using SolutionTwo.Data.UnitOfWork;
using SolutionTwo.Data.UnitOfWork.Interfaces;
using SolutionTwo.Domain.Services;
using SolutionTwo.Domain.Services.Interfaces;
using SolutionTwo.Identity.Configuration;
using SolutionTwo.Identity.DI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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

// Data:
var databaseConfiguration = builder.Configuration.GetSection<DatabaseConfiguration>();
builder.Services.AddDbContext<MainDatabaseContext>(o =>
    o.UseSqlServer(databaseConfiguration.MainDatabaseConnectionString!));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMainDatabase, MainDatabase>();

// Identity
var identityConfiguration = builder.Configuration.GetSection<IdentityConfiguration>();
builder.Services.AddIdentityServices();
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
});
builder.Services.AddAuthorization();

// Domain:
builder.Services.AddScoped<IUserService, UserService>();

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
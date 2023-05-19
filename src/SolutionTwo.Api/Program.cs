using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data;
using SolutionTwo.Data.Context;
using SolutionTwo.Data.Repositories;
using SolutionTwo.Data.Repositories.Interfaces;
using SolutionTwo.Data.UnitOfWork;
using SolutionTwo.Data.UnitOfWork.Interfaces;
using SolutionTwo.Domain.Services;
using SolutionTwo.Domain.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mainDatabaseConnectionString = builder.Configuration.GetConnectionString("MainDatabaseConnection");
builder.Services.AddDbContext<MainDatabaseContext>(o => o.UseSqlServer(mainDatabaseConnectionString));

// Data:
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMainDatabase, MainDatabase>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
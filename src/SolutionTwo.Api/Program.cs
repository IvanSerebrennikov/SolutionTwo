using SolutionTwo.Api.DI;
using SolutionTwo.Api.Extensions;
using SolutionTwo.Api.Middlewares;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Data.Common.Configuration;
using SolutionTwo.Data.MainDatabase.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// Common MultiTenancy DI
builder.Services.AddCommonMultiTenancyServices();

// Api DI
builder.Services.AddApiServices();

// Business.Common DI
builder.Services.AddBusinessCommonServices();

// Business.Identity DI
var identityConfiguration = builder.Configuration.GetSection<IdentityConfiguration>();
var useHardCodedIdentity =
    builder.Configuration.GetValue<bool?>($"{nameof(HardCodedIdentityConfiguration)}:UseHardCodedIdentity");
var hardCodedIdentityConfiguration =
    builder.Configuration.GetSection<HardCodedIdentityConfiguration>(withValidation: useHardCodedIdentity == true);
builder.Services.AddSingleton(identityConfiguration);
builder.Services.AddSingleton(hardCodedIdentityConfiguration);
builder.Services.AddBusinessIdentityServices();

// Business.MultiTenancy DI
builder.Services.AddBusinessMultiTenancyServices();

// Business.Core DI
builder.Services.AddBusinessCoreServices();

// Data DI
var connectionStrings = builder.Configuration.GetSection<ConnectionStrings>();
var mainDatabaseConfiguration = builder.Configuration.GetSection<MainDatabaseConfiguration>();
builder.Services.AddSingleton(connectionStrings);
builder.Services.AddSingleton(mainDatabaseConfiguration);
builder.Services.AddDataMainDatabaseServices(connectionStrings, mainDatabaseConfiguration);

// Build WebApp
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalErrorHandlingMiddleware>();
app.UseMiddleware<TokenBasedAuthenticationMiddleware>();
app.UseMiddleware<RoleBasedAuthorizationMiddleware>();
app.UseMiddleware<TenantAccessMiddleware>();

app.MapControllers();

app.Run();
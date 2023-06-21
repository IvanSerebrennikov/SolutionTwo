using SolutionTwo.Api.DI;
using SolutionTwo.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// Common MultiTenancy DI
builder.Services.AddCommonServices();

// Api DI
builder.Services.AddApiServices();

// Business.Common DI
builder.Services.AddBusinessCommonServices();

// Business.Identity DI
builder.Services.AddBusinessIdentityServices(builder.Configuration);

// Business.MultiTenancy DI
builder.Services.AddBusinessMultiTenancyServices();

// Business.Core DI
builder.Services.AddBusinessCoreServices();

// Data DI
builder.Services.AddDataMainDatabaseServices(builder.Configuration);

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
app.UseMiddleware<MaintenanceStatusCheckingMiddleware>();
app.UseMiddleware<TokenBasedAuthenticationMiddleware>();
app.UseMiddleware<RoleBasedAuthorizationMiddleware>();
app.UseMiddleware<TenantAccessSetupMiddleware>();

app.MapControllers();

app.Run();
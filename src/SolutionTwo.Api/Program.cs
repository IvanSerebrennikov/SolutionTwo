using SolutionTwo.Api.DI;
using SolutionTwo.Api.Extensions;
using SolutionTwo.Api.Middlewares;
using SolutionTwo.Business.Identity.Configuration;
using SolutionTwo.Data.Common.Configuration;
using SolutionTwo.Data.MainDatabase.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// MultiTenancy DI
builder.Services.AddMultiTenancyServices();

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
// Used custom TokenBasedAuthenticationMiddleware
// var identityConfiguration = builder.Configuration.GetSection<IdentityConfiguration>();
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters()
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = identityConfiguration.JwtIssuer,
//         ValidAudience = identityConfiguration.JwtAudience,
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(identityConfiguration.JwtKey!))
//     };
//     options.Events = new JwtBearerEvents
//     {
//         OnTokenValidated = ctx =>
//         {
//             var id = ctx.SecurityToken.Id;
//             var tokenManager = ctx.HttpContext.RequestServices.GetRequiredService<ITokenManager>();
//
//             if (!Guid.TryParse(id, out var authTokenId) || tokenManager.IsTokenDeactivated(authTokenId))
//             {
//                 ctx.Fail("Access was revoked.");
//             }
//             
//             return Task.CompletedTask;
//         }
//     };
// });
// Used custom RoleBasedAuthorizationMiddleware
// builder.Services.AddAuthorization();
builder.Services.AddBusinessIdentityServices();

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

// Used custom TokenBasedAuthenticationMiddleware
// app.UseAuthentication();

// Used custom RoleBasedAuthorizationMiddleware
// app.UseAuthorization();

app.MapControllers();

app.Run();
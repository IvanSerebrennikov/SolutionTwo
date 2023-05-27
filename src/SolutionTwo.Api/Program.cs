using SolutionTwo.Api.DI;
using SolutionTwo.Api.Middlewares;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.Configuration;
using SolutionTwo.Data.DI;
using SolutionTwo.Business.DI;
using SolutionTwo.Identity.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// Api DI
builder.Services.AddApiServices();

// Identity DI
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
builder.Services.AddIdentityServices();

// Business DI
builder.Services.AddBusinessServices();

// Data DI
var connectionStrings = builder.Configuration.GetSection<ConnectionStrings>();
builder.Services.AddDataServices(connectionStrings);

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

// Used custom TokenBasedAuthenticationMiddleware
// app.UseAuthentication();

// Used custom RoleBasedAuthorizationMiddleware
// app.UseAuthorization();

app.MapControllers();

app.Run();
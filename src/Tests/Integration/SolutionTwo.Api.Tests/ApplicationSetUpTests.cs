using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SolutionTwo.Api.Configuration;
using SolutionTwo.Common.Extensions;
using SolutionTwo.Data.Common.Configuration;

namespace SolutionTwo.Api.Tests;

public class ApplicationSetUpTests
{
    private readonly IConfigurationRoot _configuration;

    public ApplicationSetUpTests()
    {
        _configuration = GetConfiguration();
    }

    [SetUp]
    public void SetUp()
    {
        ClearData();
    }
    
    [TearDown]
    public void TearDown()
    {
        ClearData();
    }

    [Test]
    public async Task CreateRoles_ReturnsUnauthorized_WhenRequestedWithoutBasicAuth()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTesting");
            });
        
        var client = application.CreateClient();
        
        var createRolesUrl = "/api/application-set-up/create-roles";
        var createRolesResponse = await client.PostAsync(createRolesUrl, null);
        
        Assert.That(createRolesResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task CreateRoles_ReturnsOkWithRolesCount_WhenRequestedOnce()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTesting");
            });
        
        var client = application.CreateClient();
        var basicAuthenticationConfiguration = _configuration.GetSection<BasicAuthenticationConfiguration>();
        var credentialsString =
            Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{basicAuthenticationConfiguration.Username}:{basicAuthenticationConfiguration.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentialsString);
        
        var createRolesUrl = "/api/application-set-up/create-roles";
        var createRolesResponse = await client.PostAsync(createRolesUrl, null);

        Assert.That(createRolesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await createRolesResponse.Content.ReadAsStringAsync();
        if (int.TryParse(result, out var count) && count > 0)
        {
            Assert.Pass($"Created roles count: {count}");
        }
        else
        {
            Assert.Fail("Created roles count is not int or 0");
        }
    }
    
    [Test]
    public async Task CreateRoles_ReturnsOkWithZeroRolesCount_WhenRequestedTwice()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTesting");
            });
        
        var client = application.CreateClient();
        var basicAuthenticationConfiguration = _configuration.GetSection<BasicAuthenticationConfiguration>();
        var credentialsString =
            Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{basicAuthenticationConfiguration.Username}:{basicAuthenticationConfiguration.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentialsString);
        var createRolesUrl = "/api/application-set-up/create-roles";
        await client.PostAsync(createRolesUrl, null);
        
        var createRolesResponse = await client.PostAsync(createRolesUrl, null);

        Assert.That(createRolesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await createRolesResponse.Content.ReadAsStringAsync();
        if (int.TryParse(result, out var count) && count == 0)
        {
            Assert.Pass($"Created roles count: {count}");
        }
        else
        {
            Assert.Fail("Created roles count is not int or not 0");
        }
    }
    
    [Test]
    public async Task CreateSuperAdmin_ReturnsUnauthorized_WhenRequestedWithoutBasicAuth()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTesting");
            });
        
        var client = application.CreateClient();
        
        var createSuperAdminUrl = "/api/application-set-up/create-super-admin";
        var createSuperAdminResponse = await client.PostAsync(createSuperAdminUrl, null);
        
        Assert.That(createSuperAdminResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task CreateSuperAdmin_ReturnsBadRequest_WhenCreateRolesWasNotRequested()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("IntegrationTesting");
        });
        
        var client = application.CreateClient();
        var basicAuthenticationConfiguration = _configuration.GetSection<BasicAuthenticationConfiguration>();
        var credentialsString =
            Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{basicAuthenticationConfiguration.Username}:{basicAuthenticationConfiguration.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentialsString);
        
        var createSuperAdminUrl = "/api/application-set-up/create-super-admin";
        var createSuperAdminResponse = await client.PostAsync(createSuperAdminUrl, null);
        
        Assert.That(createSuperAdminResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task CreateSuperAdmin_ReturnsBadRequest_WhenRequestedMoreThenOnce()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTesting");
            });
        
        var client = application.CreateClient();
        var basicAuthenticationConfiguration = _configuration.GetSection<BasicAuthenticationConfiguration>();
        var credentialsString =
            Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{basicAuthenticationConfiguration.Username}:{basicAuthenticationConfiguration.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentialsString);
        var createRolesUrl = "/api/application-set-up/create-roles";
        await client.PostAsync(createRolesUrl, null);
        var createSuperAdminUrl = "/api/application-set-up/create-super-admin";
        await client.PostAsync(createSuperAdminUrl, null);
        
        var createSuperAdminResponse = await client.PostAsync(createSuperAdminUrl, null);
        
        Assert.That(createSuperAdminResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task CreateSuperAdmin_ReturnsOkWithPassword_WhenRequestedOnceAfterRolesCreation()
    {
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTesting");
            });
        
        var client = application.CreateClient();
        var basicAuthenticationConfiguration = _configuration.GetSection<BasicAuthenticationConfiguration>();
        var credentialsString =
            Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{basicAuthenticationConfiguration.Username}:{basicAuthenticationConfiguration.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentialsString);
        var createRolesUrl = "/api/application-set-up/create-roles";
        await client.PostAsync(createRolesUrl, null);
        
        var createSuperAdminUrl = "/api/application-set-up/create-super-admin";
        var createSuperAdminResponse = await client.PostAsync(createSuperAdminUrl, null);
        
        Assert.That(createSuperAdminResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await createSuperAdminResponse.Content.ReadAsStringAsync();
        if (Guid.TryParse(result, out var password) && password != Guid.Empty)
        {
            Assert.Pass("Password was returned");
        }
        else
        {
            Assert.Fail("Password was not returned");
        }
    }

    private void ClearData()
    {
        var connectionStrings = _configuration.GetSection<ConnectionStrings>();
        var connectionString = connectionStrings.MainDatabaseConnection;

        var queryString = @"DECLARE @DeleteFromTables NVARCHAR(max) = ''
                            SELECT @DeleteFromTables += 'DELETE FROM ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) + '; '
                            FROM INFORMATION_SCHEMA.TABLES
                            WHERE TABLE_NAME NOT IN ('__EFMigrationsHistory')
                            EXEC(@DeleteFromTables);";

        using var connection = new SqlConnection(connectionString);
        var command = new SqlCommand(queryString, connection);
        command.Connection.Open();
        command.ExecuteNonQuery();
    }
    
    private IConfigurationRoot GetConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.IntegrationTesting.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
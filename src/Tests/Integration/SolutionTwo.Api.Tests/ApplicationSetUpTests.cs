using System.Net;
using SolutionTwo.Api.Tests.Helpers;

namespace SolutionTwo.Api.Tests;

public class ApplicationSetUpTests
{
    [SetUp]
    public void SetUp()
    {
        DatabaseHelper.ClearAllIntegrationDatabaseTables();
    }
    
    [TearDown]
    public void TearDown()
    {
        DatabaseHelper.ClearAllIntegrationDatabaseTables();
    }

    [Test]
    public async Task CreateRoles_ReturnsUnauthorized_WhenRequestedWithoutBasicAuth()
    {
        await using var application = new TestingWebApplicationFactory();
        var client = application.CreateClient();
        
        var createRolesRequest = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateRoles);
        var createRolesResponse = await client.SendAsync(createRolesRequest);
        
        Assert.That(createRolesResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task CreateRoles_ReturnsOkWithRolesCount_WhenRequestedOnce()
    {
        await using var application = new TestingWebApplicationFactory();
        var client = application.CreateClient();
        
        var createRolesRequest = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateRoles)
            .WithBasicAuthFromConfig();
        var createRolesResponse = await client.SendAsync(createRolesRequest);
        
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
        await using var application = new TestingWebApplicationFactory();
        var client = application.CreateClient();
        var createRolesRequest1 = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateRoles)
            .WithBasicAuthFromConfig();
        await client.SendAsync(createRolesRequest1);

        var createRolesRequest2 = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateRoles)
            .WithBasicAuthFromConfig();
        var createRolesResponse2 = await client.SendAsync(createRolesRequest2);

        Assert.That(createRolesResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await createRolesResponse2.Content.ReadAsStringAsync();
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
        await using var application = new TestingWebApplicationFactory();
        var client = application.CreateClient();

        var createSuperAdminRequest = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateSuperAdmin);
        var createSuperAdminResponse = await client.SendAsync(createSuperAdminRequest);
        
        Assert.That(createSuperAdminResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task CreateSuperAdmin_ReturnsBadRequest_WhenRequestedBeforeRolesCreation()
    {
        await using var application = new TestingWebApplicationFactory();
        var client = application.CreateClient();
        
        var createSuperAdminRequest = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateSuperAdmin)
            .WithBasicAuthFromConfig();
        var createSuperAdminResponse = await client.SendAsync(createSuperAdminRequest);
        
        Assert.That(createSuperAdminResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task CreateSuperAdmin_ReturnsOkWithPassword_WhenRequestedOnceAfterRolesCreation()
    {
        await using var application = new TestingWebApplicationFactory();
        var client = application.CreateClient();
        var createRolesRequest = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateRoles)
            .WithBasicAuthFromConfig();
        await client.SendAsync(createRolesRequest);
        
        var createSuperAdminRequest = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateSuperAdmin)
            .WithBasicAuthFromConfig();
        var createSuperAdminResponse = await client.SendAsync(createSuperAdminRequest);
        
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
    
    [Test]
    public async Task CreateSuperAdmin_ReturnsBadRequest_WhenRequestedMoreThenOnceAfterRolesCreation()
    {
        await using var application = new TestingWebApplicationFactory();
        var client = application.CreateClient();
        var createRolesRequest = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateRoles)
            .WithBasicAuthFromConfig();
        await client.SendAsync(createRolesRequest);
        var createSuperAdminRequest1 = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateSuperAdmin)
            .WithBasicAuthFromConfig();
        await client.SendAsync(createSuperAdminRequest1);
        
        var createSuperAdminRequest2 = HttpRequestFactory
            .Create(HttpMethod.Post, Endpoints.ApplicationSetUp.CreateSuperAdmin)
            .WithBasicAuthFromConfig();
        var createSuperAdminResponse2 = await client.SendAsync(createSuperAdminRequest2);
        
        Assert.That(createSuperAdminResponse2.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
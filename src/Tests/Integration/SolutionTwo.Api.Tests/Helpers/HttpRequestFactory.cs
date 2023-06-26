using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace SolutionTwo.Api.Tests.Helpers;

public static class HttpRequestFactory
{
    public static HttpRequestMessage Create(HttpMethod method, string? requestUri)
    {
        return new HttpRequestMessage(method, requestUri);
    }

    public static HttpRequestMessage WithTokenBasedAuth(this HttpRequestMessage request, string tokenString)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
        
        return request;
    }
    
    public static HttpRequestMessage WithBasicAuth(this HttpRequestMessage request, string username, string password)
    {
        var credentialsString =
            Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{username}:{password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentialsString);
        
        return request;
    }
    
    public static HttpRequestMessage WithBasicAuthFromConfig(this HttpRequestMessage request)
    {
        var username = ConfigurationHelper.BasicAuthentication.Username;
        var password = ConfigurationHelper.BasicAuthentication.Password;
        
        return request.WithBasicAuth(username, password);
    }

    public static HttpRequestMessage WithContent<TContent>(this HttpRequestMessage request, TContent data)
    {
        request.Content = JsonContent.Create(data);
        
        return request;
    }
    
    public static HttpRequestMessage WithStringContent(this HttpRequestMessage request, string data)
    {
        request.Content = new StringContent(data);
        
        return request;
    }
}
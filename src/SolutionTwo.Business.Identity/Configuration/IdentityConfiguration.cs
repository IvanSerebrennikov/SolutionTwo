namespace SolutionTwo.Business.Identity.Configuration;

public class IdentityConfiguration
{
    public string? JwtKey { get; set; }

    public int? JwtExpiresMinutes { get; set; }

    public string? JwtIssuer { get; set; }

    public string? JwtAudience { get; set; }
    
    public int? RefreshTokenExpiresDays { get; set; }
}
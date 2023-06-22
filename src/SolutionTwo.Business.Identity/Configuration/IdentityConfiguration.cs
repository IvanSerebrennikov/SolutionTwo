namespace SolutionTwo.Business.Identity.Configuration;

public class IdentityConfiguration
{
    public string JwtKey { get; set; } = null!;

    public int JwtExpiresMinutes { get; set; }

    public string JwtIssuer { get; set; } = null!;

    public string JwtAudience { get; set; } = null!;
    
    public int RefreshTokenExpiresDays { get; set; }
}
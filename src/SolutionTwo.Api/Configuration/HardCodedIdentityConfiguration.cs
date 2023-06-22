namespace SolutionTwo.Api.Configuration;

public class HardCodedIdentityConfiguration
{
    public bool? UseHardCodedIdentity { get; set; }
    
    public string? Username { get; set; }

    public Guid? UserId { get; set; }
    
    public Guid? TenantId { get; set; }

    public List<string> Roles { get; set; } = new List<string>();
}
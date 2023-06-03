namespace SolutionTwo.Business.Identity.Configuration;

public class HardCodedIdentityConfiguration
{
    public bool? UseHardCodedIdentity { get; set; }
    
    public string? Username { get; set; }

    public List<string> Roles { get; set; } = new List<string>();
}
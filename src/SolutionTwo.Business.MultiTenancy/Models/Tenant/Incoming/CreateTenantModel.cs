using System.ComponentModel.DataAnnotations;

namespace SolutionTwo.Business.MultiTenancy.Models.Tenant.Incoming;

public class CreateTenantModel
{
    [Required]
    public string TenantName { get; set; } = null!;
    
    [Required]
    public string AdminFirstName { get; set; } = null!;

    [Required]
    public string AdminLastName { get; set; } = null!;

    [Required]
    public string AdminUsername { get; set; } = null!;

    [Required]
    public string AdminPassword { get; set; } = null!;
}
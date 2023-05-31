using System.ComponentModel.DataAnnotations;

namespace SolutionTwo.Business.Identity.Models.Auth.Incoming;

public class UserCredentialsModel
{
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
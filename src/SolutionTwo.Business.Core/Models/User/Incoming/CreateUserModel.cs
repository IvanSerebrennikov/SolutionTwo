using System.ComponentModel.DataAnnotations;

namespace SolutionTwo.Business.Core.Models.User.Incoming;

public class CreateUserModel
{
    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
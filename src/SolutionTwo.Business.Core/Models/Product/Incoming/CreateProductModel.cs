using System.ComponentModel.DataAnnotations;

namespace SolutionTwo.Business.Core.Models.Product.Incoming;

public class CreateProductModel
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue)]
    public int MaxNumberOfSimultaneousUsages { get; set; }
}
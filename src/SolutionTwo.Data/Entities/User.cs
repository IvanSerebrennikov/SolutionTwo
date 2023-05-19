using SolutionTwo.Data.Entities.Base.Interfaces;

namespace SolutionTwo.Data.Entities;

public class User : IIdentifiablyEntity<Guid>
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;
}
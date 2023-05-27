using SolutionTwo.Data.Entities.Base.Interfaces;

namespace SolutionTwo.Business.Tests.InMemoryRepositories.EntityComparer;

public class IdentifiablyEntityComparer<TId> : IEqualityComparer<IIdentifiablyEntity<TId>>
{
    public bool Equals(IIdentifiablyEntity<TId>? x, IIdentifiablyEntity<TId>? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return Equals(x.Id, y.Id);
    }

    public int GetHashCode(IIdentifiablyEntity<TId> obj)
    {
        return EqualityComparer<TId>.Default.GetHashCode(obj.Id!);
    }
}
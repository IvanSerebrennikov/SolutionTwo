using System.Linq.Expressions;
using SolutionTwo.Data.Entities.Base.Interfaces;
using SolutionTwo.Data.Repositories.Base.Interfaces;

namespace SolutionTwo.Data.Repositories.Base;

public class BaseRepository<TEntity, TId> : IBaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{
    public TEntity GetById(TId id)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<TEntity> Get(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, 
        string? includeProperties = null,
        int? skip = null,
        int? take = null)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<TProjection> GetProjections<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null, 
        int? skip = null, 
        int? take = null)
    {
        throw new NotImplementedException();
    }

    public void Create(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public void Update(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(TId id)
    {
        throw new NotImplementedException();
    }
}
using System.Linq.Expressions;
using SolutionTwo.Data.Entities.Base.Interfaces;

namespace SolutionTwo.Data.Repositories.Base.Interfaces;

public interface IBaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{
    TEntity GetById(TId id);

    IReadOnlyList<TEntity> Get(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null);

    IReadOnlyList<TProjection> GetProjections<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null);

    void Create(TEntity entity);

    void Update(TEntity entity);

    void Delete(TEntity entity);

    void Delete(TId id);
}
using System.Linq.Expressions;
using SolutionTwo.Data.Entities.Base.Interfaces;

namespace SolutionTwo.Data.Repositories.Base.Interfaces;

public interface IBaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, bool asNoTracking = false);

    Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includeProperties = null,
        int? skip = null,
        int? take = null, 
        bool asNoTracking = false);
    
    Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> filter,
        bool asNoTracking);

    Task<IReadOnlyList<TProjection>> GetProjectionsAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includeProperties = null,
        int? skip = null,
        int? take = null, 
        bool asNoTracking = false);

    Task CreateAsync(TEntity entity);

    void Update(TEntity entity);

    void Delete(TEntity entity);

    Task DeleteAsync(TId id);
}
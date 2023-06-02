using System.Linq.Expressions;
using SolutionTwo.Data.Common.Entities.Interfaces;

namespace SolutionTwo.Data.Common.Repositories.Interfaces;

public interface IBaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{
    Task<TEntity?> GetByIdAsync(
        TId id, 
        string? includeProperties = null,
        bool withTracking = false);

    Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        string? includeProperties = null,
        bool withTracking = false);
    
    Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null, 
        bool withTracking = false);

    Task<IReadOnlyList<TProjection>> GetProjectionsAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null, 
        bool withTracking = false);

    void Create(TEntity entity);

    void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties);

    void Delete(TEntity entity);

    Task DeleteAsync(TId id);
}
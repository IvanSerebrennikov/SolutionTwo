using System.Linq.Expressions;
using SolutionTwo.Data.Common.Entities.Interfaces;

namespace SolutionTwo.Data.Common.Repositories.Interfaces;

public interface IBaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{
    Task<TEntity?> GetByIdAsync(
        TId id, 
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        bool withTracking = false);

    Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        bool withTracking = false);

    Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        int? skip = null,
        int? take = null, 
        bool withTracking = false);

    Task<IReadOnlyList<TProjection>> GetProjectionsAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        int? skip = null,
        int? take = null, 
        bool withTracking = false);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter);
    
    void Create(TEntity entity);

    void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties);

    void Delete(TEntity entity);

    Task DeleteAsync(TId id);
}
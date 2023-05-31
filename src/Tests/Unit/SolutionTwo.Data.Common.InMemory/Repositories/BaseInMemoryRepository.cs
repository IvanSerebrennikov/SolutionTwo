using System.Linq.Expressions;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.InMemory.EntityComparer;
using SolutionTwo.Data.Common.Repositories.Interfaces;

namespace SolutionTwo.Data.Common.InMemory.Repositories;

// Changes are applied instantly, without UnitOfWork.CommitChanges

// includeProperties is not supported, entities return with all nested data

// asNoTracking is not supported, behavior like all entities are tracking 

// orderBy is not supported
// (if ordering will be really required for some test cases -
// unique method can be added to related I[Entity]Repository interface and realised in both EF and InMemory implementations)

public class BaseInMemoryRepository<TEntity, TId> : IBaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{
    private readonly HashSet<TEntity> _entities = new(new IdentifiablyEntityComparer<TId>());

    public async Task<TEntity?> GetByIdAsync(TId id, string? includeProperties = null, bool asNoTracking = false)
    {
        var result = GetEnumerable(x => x.Id!.Equals(id), null, includeProperties, null, null, asNoTracking)
            .FirstOrDefault();
        return await Task.FromResult(result);
    }

    public async Task<TEntity?> GetSingleAsync(Expression<Func<TEntity, bool>> filter, string? includeProperties = null,
        bool asNoTracking = false)
    {
        var result = GetEnumerable(filter, null, includeProperties, null, null, asNoTracking)
            .SingleOrDefault();
        return await Task.FromResult(result);
    }

    public async Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, string? includeProperties = null,
        int? skip = null,
        int? take = null, bool asNoTracking = false)
    {
        var result = GetEnumerable(filter, orderBy, includeProperties, skip, take, asNoTracking)
            .ToList();
        return await Task.FromResult(result);
    }

    public async Task<IReadOnlyList<TProjection>> GetProjectionsAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection, Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null, int? skip = null, int? take = null, bool asNoTracking = false)
    {
        var result = GetEnumerable(filter, orderBy, includeProperties, skip, take, asNoTracking)
            .Select(projection.Compile())
            .ToList();
        return await Task.FromResult(result);
    }

    public void Create(TEntity entity)
    {
        _entities.Add(entity);
    }

    public void Update(TEntity entity)
    {
    }

    public void Delete(TEntity entity)
    {
        _entities.Remove(entity);
    }

    public async Task DeleteAsync(TId id)
    {
        var entity = await GetByIdAsync(id);

        if (entity != null) Delete(entity);
    }

    protected virtual IEnumerable<TEntity> GetEnumerable(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = false)
    {
        IEnumerable<TEntity> query = _entities;

        if (filter != null) query = query.Where(filter.Compile());

        if (skip != null) query = query.Skip(skip.Value);

        if (take != null) query = query.Take(take.Value);

        return query;
    }
}
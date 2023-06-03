using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.Repositories.Interfaces;

namespace SolutionTwo.Data.Common.Repositories;

public abstract class BaseRepository<TEntity, TId> : IBaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{
    protected readonly DbContext Context;

    protected BaseRepository(DbContext context)
    {
        Context = context;
    }

    public async Task<TEntity?> GetByIdAsync(
        TId id, 
        string? includeProperties = null,
        Expression<Func<TEntity, object>>? include = null,
        bool withTracking = false)
    {
        return await GetQueryable(x => x.Id!.Equals(id), null, includeProperties, include, null, null, withTracking)
            .FirstOrDefaultAsync();
    }

    public async Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        string? includeProperties = null, 
        Expression<Func<TEntity, object>>? include = null,
        bool withTracking = false)
    {
        return await GetQueryable(filter, null, includeProperties, include, null, null, withTracking)
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null,
        Expression<Func<TEntity, object>>? include = null,
        int? skip = null,
        int? take = null,
        bool withTracking = false)
    {
        return await GetQueryable(filter, orderBy, includeProperties, include, skip, take, withTracking)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TProjection>> GetProjectionsAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null,
        Expression<Func<TEntity, object>>? include = null,
        int? skip = null,
        int? take = null,
        bool withTracking = false)
    {
        return await GetQueryable(filter, orderBy, includeProperties, include, skip, take, withTracking)
            .Select(projection)
            .ToListAsync();
    }

    public void Create(TEntity entity)
    {
        Context.Set<TEntity>().Add(entity);
    }

    public void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties)
    {
        var dbEntityEntry = Context.Entry(entity);
        if (updatedProperties.Any())
        {
            foreach (var property in updatedProperties)
            {
                dbEntityEntry.Property(property).IsModified = true;
            }
        }
        else
        {
            Context.Set<TEntity>().Update(entity);
        }
    }

    public void Delete(TEntity entity)
    {
        if (entity is ISoftDeletableEntity softDeletableEntity)
        {
            softDeletableEntity.IsDeleted = true;
            var dbEntityEntry = Context.Entry(entity);
            dbEntityEntry.Property(nameof(softDeletableEntity.IsDeleted)).IsModified = true;
        }
        else
        {
            Context.Set<TEntity>().Remove(entity);
        }
    }

    public async Task DeleteAsync(TId id)
    {
        var entity = await GetByIdAsync(id);

        if (entity != null) Delete(entity);
    }

    protected virtual IQueryable<TEntity> GetQueryable(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null,
        Expression<Func<TEntity, object>>? include = null, 
        int? skip = null,
        int? take = null,
        bool withTracking = false)
    {
        IQueryable<TEntity> query = Context.Set<TEntity>();

        if (withTracking) query = query.AsTracking();

        if (filter != null) query = query.Where(filter);
        
        var filterForEachQuery = GetFilterForEachQuery();
        if (filterForEachQuery != null) 
            query = query.Where(filterForEachQuery);
        
        if (include != null)
        {
            query = query.Include(include);
        }
        
        if (!string.IsNullOrEmpty(includeProperties))
        {
            query = includeProperties
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query,
                    (current, property) => current.Include(property.Trim()));
        }

        if (orderBy != null) query = orderBy(query);

        if (skip != null) query = query.Skip(skip.Value);

        if (take != null) query = query.Take(take.Value);

        return query;
    }

    protected virtual Expression<Func<TEntity, bool>>? GetFilterForEachQuery()
    {
        return null;
    }
}
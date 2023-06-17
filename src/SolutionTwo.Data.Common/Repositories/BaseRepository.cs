using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.Interfaces;
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
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        bool withTracking = false)
    {
        return await GetQueryable(x => x.Id!.Equals(id), null, includeMany, include, null, null, withTracking)
            .FirstOrDefaultAsync();
    }

    public async Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        string? includeMany = null, 
        Expression<Func<TEntity, object>>? include = null,
        bool withTracking = false)
    {
        return await GetQueryable(filter, null, includeMany, include, null, null, withTracking)
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        int? skip = null,
        int? take = null,
        bool withTracking = false)
    {
        return await GetQueryable(filter, orderBy, includeMany, include, skip, take, withTracking)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TProjection>> GetProjectionsAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null,
        int? skip = null,
        int? take = null,
        bool withTracking = false)
    {
        return await GetQueryable(filter, orderBy, includeMany, include, skip, take, withTracking)
            .Select(projection)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await GetQueryable(filter).AnyAsync();
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
        Context.Set<TEntity>().Remove(entity);
    }

    public async Task DeleteAsync(TId id)
    {
        var entity = await GetByIdAsync(id);

        if (entity != null) Delete(entity);
    }

    protected virtual IQueryable<TEntity> GetQueryable(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeMany = null,
        Expression<Func<TEntity, object>>? include = null, 
        int? skip = null,
        int? take = null,
        bool withTracking = false)
    {
        IQueryable<TEntity> query = Context.Set<TEntity>();

        if (withTracking) query = query.AsTracking();

        if (filter != null) query = query.Where(filter);

        if (include != null)
        {
            query = query.Include(include);
        }
        
        if (!string.IsNullOrEmpty(includeMany))
        {
            query = includeMany
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query,
                    (current, property) => current.Include(property.Trim()));
        }

        if (orderBy != null) query = orderBy(query);

        if (skip != null) query = query.Skip(skip.Value);

        if (take != null) query = query.Take(take.Value);

        return query;
    }
}
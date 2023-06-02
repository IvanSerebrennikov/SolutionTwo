using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Common.Entities.Interfaces;
using SolutionTwo.Data.Common.Repositories.Interfaces;

namespace SolutionTwo.Data.Common.Repositories;

public abstract class BaseRepository<TEntity, TId> : IBaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{
    private readonly DbContext _context;

    protected BaseRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<TEntity?> GetByIdAsync(
        TId id, 
        string? includeProperties = null,
        bool withTracking = false)
    {
        return await GetQueryable(x => x.Id!.Equals(id), null, includeProperties, null, null, withTracking)
            .FirstOrDefaultAsync();
    }

    public async Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        string? includeProperties = null, 
        bool withTracking = false)
    {
        return await GetQueryable(filter, null, includeProperties, null, null, withTracking)
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null,
        bool withTracking = false)
    {
        return await GetQueryable(filter, orderBy, includeProperties, skip, take, withTracking)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TProjection>> GetProjectionsAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null,
        bool withTracking = false)
    {
        return await GetQueryable(filter, orderBy, includeProperties, skip, take, withTracking).Select(projection)
            .ToListAsync();
    }

    public void Create(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
    }

    public void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties)
    {
        var dbEntityEntry = _context.Entry(entity);
        if (updatedProperties.Any())
        {
            foreach (var property in updatedProperties)
            {
                dbEntityEntry.Property(property).IsModified = true;
            }
        }
        else
        {
            _context.Set<TEntity>().Update(entity);
        }
    }

    public void Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
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
        int? skip = null,
        int? take = null,
        bool withTracking = false)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        if (withTracking) query = query.AsTracking();

        if (filter != null) query = query.Where(filter);

        if (!string.IsNullOrEmpty(includeProperties))
        {
            query = includeProperties
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query,
                    (current, includeProperty) => current.Include(includeProperty.Trim()));
        }

        if (orderBy != null) query = orderBy(query);

        if (skip != null) query = query.Skip(skip.Value);

        if (take != null) query = query.Take(take.Value);

        return query;
    }
}
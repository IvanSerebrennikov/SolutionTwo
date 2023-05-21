using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SolutionTwo.Data.Context;
using SolutionTwo.Data.Entities.Base.Interfaces;
using SolutionTwo.Data.Repositories.Base.Interfaces;

namespace SolutionTwo.Data.Repositories.Base;

public abstract class BaseRepository<TEntity, TId> : IBaseRepository<TEntity, TId>
    where TEntity : class, IIdentifiablyEntity<TId>
{
    private readonly MainDatabaseContext _context;

    protected BaseRepository(MainDatabaseContext context)
    {
        _context = context;
    }

    public async Task<TEntity?> GetByIdAsync(TId id, bool asNoTracking = false)
    {
        return await GetQueryable(x => x.Id!.Equals(id), null, null, null, null, asNoTracking)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includeProperties = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = false)
    {
        return await GetQueryable(filter, orderBy, includeProperties, skip, take, asNoTracking)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> filter,
        bool asNoTracking)
    {
        return await GetQueryable(filter, null, null, null, null, asNoTracking)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TProjection>> GetProjectionsAsync<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includeProperties = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = false)
    {
        return await GetQueryable(filter, orderBy, includeProperties, skip, take, asNoTracking).Select(projection)
            .ToListAsync();
    }

    public async Task CreateAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
    }

    public void Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
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
        IEnumerable<Expression<Func<TEntity, object>>>? includeProperties = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = false)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        if (asNoTracking) query = query.AsNoTracking();

        if (filter != null) query = query.Where(filter);

        if (includeProperties != null)
            query = includeProperties
                .Aggregate(query,
                    (current, expression) => current.Include(expression));

        if (orderBy != null) query = orderBy(query);

        if (skip != null) query = query.Skip(skip.Value);

        if (take != null) query = query.Take(take.Value);

        return query;
    }
}
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tenon.Extensions.Collection;
using Tenon.Extensions.Expression;

namespace Tenon.Repository.EfCore;

public class EfRepository<TEntity>(DbContext dbContext) : IRepository<TEntity, long>, IEfRepository<TEntity, long>
    where TEntity : EfEntity, new()
{
    protected readonly DbContext DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <summary>
    ///     根据条件查询
    /// </summary>
    public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool noTracking = true)
    {
        return GetDbSet(noTracking).Where(expression);
    }

    /// <summary>
    ///     异步获取列表
    /// </summary>
    /// <param name="whereExpression">查询条件</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> whereExpression,
        bool noTracking = true, CancellationToken token = default)
    {
        var query = whereExpression != null ? GetDbSet(noTracking).Where(whereExpression) : GetDbSet(noTracking);
        return await query.ToListAsync(token);
    }

    /// <summary>
    ///     异步根据主键获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<TEntity?> GetAsync(long keyValue, bool noTracking = true,
        CancellationToken token = default)
    {
        return await GetAsync(keyValue, null, noTracking, token);
    }

    /// <summary>
    ///     异步根据主键和导航属性获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="navigationPropertyPaths">导航属性路径</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    public async Task<TEntity?> GetWithNavigationPropertiesAsync(long keyValue,
        IEnumerable<Expression<Func<TEntity, dynamic>>> navigationPropertyPaths = null, bool noTracking = true,
        CancellationToken token = default)
    {
        if (navigationPropertyPaths is null)
            return await GetAsync(keyValue, null, noTracking, token);
        if (navigationPropertyPaths.Count() == 1)
            return await GetAsync(keyValue, navigationPropertyPaths.First(), noTracking, token);
        var query = GetDbSet(noTracking).Where(t => t.Id == keyValue);
        foreach (var navigationPath in navigationPropertyPaths)
            query = query.Include(navigationPath);
        return await query.FirstOrDefaultAsync(token);
    }


    /// <summary>
    ///     异步根据主键和单个导航属性获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="navigationPropertyPath">导航属性路径</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<TEntity?> GetAsync(long keyValue,
        Expression<Func<TEntity, dynamic>> navigationPropertyPath = null, bool noTracking = true,
        CancellationToken token = default)
    {
        var query = GetDbSet(false).Where(t => t.Id.Equals(keyValue));
        if (navigationPropertyPath is null)
            return await query.FirstOrDefaultAsync(token);

        return await query.Include(navigationPropertyPath).FirstOrDefaultAsync(token);
    }

    /// <summary>
    ///     获取所有实体
    /// </summary>
    /// <param name="noTracking">是否不追踪</param>
    public virtual IQueryable<TEntity> GetAll(bool noTracking = true)
    {
        return GetDbSet(noTracking);
    }


    public virtual async Task<int> InsertAsync(TEntity entity, CancellationToken token = default)
    {
        await DbContext.Set<TEntity>().AddAsync(entity, token);
        return await DbContext.SaveChangesAsync(token);
    }

    public virtual async Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken token = default)
    {
        await DbContext.Set<TEntity>().AddRangeAsync(entities, token);
        return await DbContext.SaveChangesAsync(token);
    }

    public virtual async Task<int> UpdateAsync(TEntity entity, CancellationToken token = default)
    {
        var entry = DbContext.Entry(entity);
        if (entry.State == EntityState.Detached)
            throw new InvalidOperationException("Entity is not tracked, need to specify updated columns");

        if (entry.State is EntityState.Added or EntityState.Deleted)
            throw new InvalidOperationException($"{nameof(entity)},The entity state is {nameof(entry.State)}");

        return await DbContext.SaveChangesAsync(token);
    }

    public virtual async Task<int> UpdateAsync(IEnumerable<TEntity> entities, CancellationToken token = default)
    {
        foreach (var entity in entities)
        {
            var entry = DbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                throw new InvalidOperationException("Entity is not tracked, need to specify updated columns");

            if (entry.State is EntityState.Added or EntityState.Deleted)
                throw new InvalidOperationException($"{nameof(entity)},The entity state is {nameof(entry.State)}");
        }

        return await DbContext.SaveChangesAsync(token);
    }


    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression,
        CancellationToken token = default)
    {
        var dbSet = DbContext.Set<TEntity>().AsNoTracking();
        return await dbSet.AnyAsync(whereExpression, token);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression,
        CancellationToken token = default)
    {
        var dbSet = DbContext.Set<TEntity>().AsNoTracking();
        return await dbSet.CountAsync(whereExpression, token);
    }

    public virtual async Task<int> RemoveAsync(TEntity entity, CancellationToken token = default)
    {
        DbContext.Remove(entity);
        return await DbContext.SaveChangesAsync(token);
    }

    public virtual async Task<int> RemoveAsync(long keyValue, CancellationToken token = default)
    {
        var entity = DbContext.Set<TEntity>().AsNoTracking()
                         .FirstOrDefault(t => t.Id.Equals(keyValue)) ??
                     new TEntity {Id = keyValue};
        DbContext.Remove(entity);
        try
        {
            return await DbContext.SaveChangesAsync(token);
        }
        catch (DbUpdateConcurrencyException)
        {
            return 0;
        }
    }

    public virtual async Task<int> RemoveAsync(IEnumerable<TEntity> entities, CancellationToken token = default)
    {
        DbContext.RemoveRange(entities);
        return await DbContext.SaveChangesAsync(token);
    }

    public virtual async Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>>[] updatingExpressions,
        CancellationToken token = default)
    {
        if (updatingExpressions.IsNullOrEmpty())
            await UpdateAsync(entity, token);
        var entry = DbContext.Entry(entity);
        if (entry.State == EntityState.Detached)
            throw new InvalidOperationException("Entity is not tracked, need to specify updated columns");

        if (entry.State == EntityState.Added || entry.State == EntityState.Deleted)
            throw new InvalidOperationException($"{nameof(entity)},The entity state is {nameof(entry.State)}");

        if (entry.State == EntityState.Modified)
        {
            var propNames = updatingExpressions.Select(x => x.GetMemberName()).ToArray();
            foreach (var propEntry in entry.Properties)
                if (!propNames.Contains(propEntry.Metadata.Name))
                    propEntry.IsModified = false;
        }

        if (entry.State == EntityState.Detached)
        {
            entry.State = EntityState.Unchanged;
            foreach (var expression in updatingExpressions)
                entry.Property(expression).IsModified = true;
        }

        return await DbContext.SaveChangesAsync(token);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await GetAll().ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> whereExpression,
        CancellationToken token = default)
    {
        return await GetListAsync(whereExpression, true, token);
    }

    public virtual async Task<TEntity> GetAsync(long keyValue, CancellationToken token = default)
    {
        return await GetAsync(keyValue, true, token);
    }

    public virtual async Task<TEntity> GetAsync(long keyValue,
        IEnumerable<Expression<Func<TEntity, dynamic>>> navigationPropertyPaths = null,
        CancellationToken token = default)
    {
        return await GetWithNavigationPropertiesAsync(keyValue, navigationPropertyPaths, true, token);
    }

    public virtual async Task<TEntity> GetAsync(long keyValue,
        Expression<Func<TEntity, dynamic>> navigationPropertyPath = null, CancellationToken token = default)
    {
        return await GetAsync(keyValue, navigationPropertyPath, true, token);
    }


    protected virtual IQueryable<TEntity> GetDbSet(bool noTracking)
    {
        return noTracking ? DbContext.Set<TEntity>().AsNoTracking() : DbContext.Set<TEntity>();
    }
}
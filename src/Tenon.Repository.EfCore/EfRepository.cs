using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tenon.Extensions.Collection;
using Tenon.Extensions.Expression;

namespace Tenon.Repository.EfCore;

/// <summary>
/// EF Core 仓储实现
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public class EfRepository<TEntity> : IRepository<TEntity, long>, IEfRepository<TEntity, long>
    where TEntity : EfEntity, new()
{
    protected readonly DbContext DbContext;
    private readonly ILogger<EfRepository<TEntity>> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="logger">日志记录器</param>
    public EfRepository(DbContext dbContext, ILogger<EfRepository<TEntity>> logger)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger;
    }

    /// <summary>
    /// 根据条件查询
    /// </summary>
    /// <param name="expression">查询条件表达式</param>
    /// <param name="noTracking">是否不追踪实体</param>
    public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool noTracking = true)
    {
        return GetDbSet(noTracking).Where(expression);
    }

    /// <summary>
    /// 异步获取列表
    /// </summary>
    /// <param name="whereExpression">查询条件表达式</param>
    /// <param name="navigationPropertyPath">包含的导航属性</param>
    /// <param name="noTracking">是否不追踪实体</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> whereExpression,
        Expression<Func<TEntity, dynamic>>? navigationPropertyPath = null,
        bool noTracking = true, CancellationToken token = default)
    {
        var query = whereExpression != null ? GetDbSet(noTracking).Where(whereExpression) : GetDbSet(noTracking);
        if (navigationPropertyPath != null) query = query.Include(navigationPropertyPath);
        return await query.ToListAsync(token).ConfigureAwait(false);
    }


    /// <summary>
    /// 异步获取符合条件的列表，并包含指定的导航属性
    /// </summary>
    /// <param name="whereExpression">查询条件</param>
    /// <param name="navigationPropertyPaths">要包含的导航属性路径</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    public async Task<IEnumerable<TEntity>> GetListWithNavigationPropertiesAsync(
        Expression<Func<TEntity, bool>> whereExpression,
        IEnumerable<Expression<Func<TEntity, dynamic>>>? navigationPropertyPaths = null,
        bool noTracking = true, CancellationToken token = default)
    {
        var query = whereExpression != null ? GetDbSet(noTracking).Where(whereExpression) : GetDbSet(noTracking);
        if (navigationPropertyPaths != null)
            foreach (var navigationPath in navigationPropertyPaths)
                query = query.Include(navigationPath);
        return await query.ToListAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步根据主键获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="noTracking">是否不追踪实体</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<TEntity?> GetAsync(long keyValue, bool noTracking = true,
        CancellationToken token = default)
    {
        var query = GetDbSet(noTracking).Where(t => t.Id.Equals(keyValue));
        return await query.FirstOrDefaultAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步根据主键和导航属性获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="navigationPropertyPaths">导航属性路径集合</param>
    /// <param name="noTracking">是否不追踪实体</param>
    /// <param name="token">取消令牌</param>
    public async Task<TEntity?> GetWithNavigationPropertiesAsync(long keyValue,
        IEnumerable<Expression<Func<TEntity, dynamic>>>? navigationPropertyPaths = null, bool noTracking = true,
        CancellationToken token = default)
    {
        var query = GetDbSet(noTracking).Where(t => t.Id == keyValue);
        if (navigationPropertyPaths != null)
            foreach (var navigationPath in navigationPropertyPaths)
                query = query.Include(navigationPath);
        return await query.FirstOrDefaultAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// 根据主键和单个导航属性获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="navigationPropertyPath">导航属性路径</param>
    /// <param name="noTracking">是否不追踪实体</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<TEntity?> GetAsync(long keyValue,
        Expression<Func<TEntity, dynamic>>? navigationPropertyPath = null,
        bool noTracking = true, CancellationToken token = default)
    {
        if (navigationPropertyPath != null)
            return await GetWithNavigationPropertiesAsync(keyValue, [navigationPropertyPath], noTracking, token)
                .ConfigureAwait(false);
        return await GetAsync(keyValue, noTracking, token).ConfigureAwait(false);
    }

    /// <summary>
    /// 获取所有实体
    /// </summary>
    /// <param name="noTracking">是否不追踪实体</param>
    public virtual IQueryable<TEntity> GetAll(bool noTracking = true)
    {
        return GetDbSet(noTracking);
    }

    /// <summary>
    /// 异步获取分页列表
    /// </summary>
    /// <param name="whereExpression">查询条件表达式</param>
    /// <param name="pageIndex">当前页索引</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="includeProperties">要包含的导航属性路径</param>
    /// <param name="noTracking">是否不追踪实体</param>
    /// <param name="token">取消令牌</param>
    /// <returns>分页结果</returns>
    public async Task<PagedResult<TEntity>> GetPagedListAsync(Expression<Func<TEntity, bool>> whereExpression,
        int pageIndex, int pageSize, IEnumerable<Expression<Func<TEntity, dynamic>>>? includeProperties = null,
        bool noTracking = true, CancellationToken token = default)
    {
        var query = whereExpression != null ? GetDbSet(noTracking).Where(whereExpression) : GetDbSet(noTracking);

        if (includeProperties != null)
            foreach (var includeProperty in includeProperties)
                query = query.Include(includeProperty);

        var totalCount = await query.CountAsync(token).ConfigureAwait(false);
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(token)
            .ConfigureAwait(false);

        return new PagedResult<TEntity>
        {
            TotalCount = totalCount,
            Items = items,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 异步插入单个实体
    /// </summary>
    /// <param name="entity">要插入的实体</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<int> InsertAsync(TEntity entity, CancellationToken token = default)
    {
        await DbContext.Set<TEntity>().AddAsync(entity, token).ConfigureAwait(false);
        return await DbContext.SaveChangesAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步插入多个实体
    /// </summary>
    /// <param name="entities">要插入的实体集合</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken token = default)
    {
        await DbContext.Set<TEntity>().AddRangeAsync(entities, token).ConfigureAwait(false);
        return await DbContext.SaveChangesAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步更新单个实体
    /// </summary>
    /// <param name="entity">要更新的实体</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<int> UpdateAsync(TEntity entity, CancellationToken token = default)
    {
        var entry = DbContext.Entry(entity);
        if (entry.State == EntityState.Detached)
            throw new InvalidOperationException("Entity is not tracked, need to specify updated columns");

        if (entry.State is EntityState.Added or EntityState.Deleted)
            throw new InvalidOperationException($"{nameof(entity)}, The entity state is {nameof(entry.State)}");

        return await DbContext.SaveChangesAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步更新多个实体
    /// </summary>
    /// <param name="entities">要更新的实体集合</param>
    /// <param name="token">取消令牌</param>
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

    /// <summary>
    /// 异步检查是否存在满足条件的实体
    /// </summary>
    /// <param name="whereExpression">查询条件表达式</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression,
        CancellationToken token = default)
    {
        _logger.LogDebug("开始检查是否存在满足条件的实体，查询条件: {WhereExpression}", whereExpression);
        var exists = await DbContext.Set<TEntity>().AsNoTracking().AnyAsync(whereExpression, token);
        _logger.LogDebug("检查结果: {Exists}", exists);
        return exists;
    }

    /// <summary>
    /// 异步获取满足条件的实体数量
    /// </summary>
    /// <param name="whereExpression">查询条件表达式</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression,
        CancellationToken token = default)
    {
        _logger.LogDebug("开始获取满足条件的实体数量，查询条件: {WhereExpression}", whereExpression);
        var count = await DbContext.Set<TEntity>().AsNoTracking().CountAsync(whereExpression, token);
        _logger.LogDebug("满足条件的实体数量: {Count}", count);
        return count;
    }

    /// <summary>
    /// 异步删除单个实体
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<int> RemoveAsync(TEntity entity, CancellationToken token = default)
    {
        DbContext.Remove(entity);
        return await DbContext.SaveChangesAsync(token);
    }

    /// <summary>
    /// 异步根据主键删除实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<int> RemoveAsync(long keyValue, CancellationToken token = default)
    {
        try
        {
            _logger.LogDebug("开始根据主键删除实体，主键值: {KeyValue}", keyValue);
            var entity = await DbContext.Set<TEntity>().AsNoTracking()
                             .FirstOrDefaultAsync(t => t.Id.Equals(keyValue), token).ConfigureAwait(false) ??
                         new TEntity { Id = keyValue };
            DbContext.Remove(entity);
            var result = await DbContext.SaveChangesAsync(token).ConfigureAwait(false);
            _logger.LogDebug("成功删除实体，主键值: {KeyValue}", keyValue);
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "删除实体时发生并发错误，主键值: {KeyValue}", keyValue);
            return 0;
        }
    }

    /// <summary>
    /// 异步删除多个实体
    /// </summary>
    /// <param name="entities">要删除的实体集合</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<int> RemoveAsync(IEnumerable<TEntity> entities, CancellationToken token = default)
    {
        DbContext.RemoveRange(entities);
        return await DbContext.SaveChangesAsync(token);
    }

    /// <summary>
    /// 异步更新实体的指定属性
    /// </summary>
    /// <param name="entity">要更新的实体</param>
    /// <param name="updatingExpressions">指定要更新的属性表达式数组</param>
    /// <param name="token">取消令牌</param>
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

    /// <summary>
    /// 异步获取所有实体
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await GetAll().ToListAsync();
    }

    /// <summary>
    /// 异步获取满足条件的实体列表
    /// </summary>
    /// <param name="whereExpression">查询条件表达式</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> whereExpression,
        CancellationToken token = default)
    {
        var result = await GetListAsync(whereExpression, null, true, token);
        return result;
    }

    /// <summary>
    /// 异步根据主键获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<TEntity?> GetAsync(long keyValue, CancellationToken token = default)
    {
        _logger.LogDebug("开始根据主键获取实体，主键值: {KeyValue}", keyValue);
        var entity = await GetAsync(keyValue, true, token);
        _logger.LogDebug("成功获取实体: {Entity}", entity);
        return entity;
    }

    /// <summary>
    /// 异步根据主键和导航属性路径获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="navigationPropertyPaths">导航属性路径集合</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<TEntity?> GetAsync(long keyValue,
        IEnumerable<Expression<Func<TEntity, dynamic>>>? navigationPropertyPaths = null,
        CancellationToken token = default)
    {
        return await GetWithNavigationPropertiesAsync(keyValue, navigationPropertyPaths, true, token);
    }

    /// <summary>
    /// 异步根据主键和单个导航属性路径获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="navigationPropertyPath">导航属性路径</param>
    /// <param name="token">取消令牌</param>
    public virtual async Task<TEntity?> GetByKeyWithNavigationAsync(long keyValue,
        Expression<Func<TEntity, dynamic>>? navigationPropertyPath = null, CancellationToken token = default)
    {
        return await GetAsync(keyValue, navigationPropertyPath, true, token);
    }

    /// <summary>
    /// 获取数据库集合
    /// </summary>
    /// <param name="noTracking">是否不追踪实体</param>
    protected virtual IQueryable<TEntity> GetDbSet(bool noTracking)
    {
        return noTracking ? DbContext.Set<TEntity>().AsNoTracking() : DbContext.Set<TEntity>();
    }
}
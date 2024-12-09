using System.Linq.Expressions;

namespace Tenon.Repository.EfCore;

public interface IEfRepository<TEntity, in TKey> where TEntity : IEntity<TKey>
{
    /// <summary>
    /// 根据条件查询
    /// </summary>
    IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> expression, bool noTracking = true);

    /// <summary>
    /// 异步获取列表
    /// </summary>
    /// <param name="whereExpression">查询条件</param>
    /// <param name="navigationPropertyPath">包含的导航属性</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> whereExpression, 
        Expression<Func<TEntity, dynamic>>? navigationPropertyPath = null, bool noTracking = true,
        CancellationToken token = default);

    
    /// <summary>
    /// 异步获取符合条件的列表，并包含指定的导航属性
    /// </summary>
    /// <param name="whereExpression">查询条件</param>
    /// <param name="navigationPropertyPaths">要包含的导航属性路径</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    Task<IEnumerable<TEntity>> GetListWithNavigationPropertiesAsync(Expression<Func<TEntity, bool>> whereExpression, 
        IEnumerable<Expression<Func<TEntity, dynamic>>>? navigationPropertyPaths = null, bool noTracking = true,
        CancellationToken token = default);


    /// <summary>
    /// 异步根据主键获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="navigationPropertyPath">导航属性路径</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    Task<TEntity?> GetAsync(TKey keyValue, Expression<Func<TEntity, dynamic>>? navigationPropertyPath = null,
        bool noTracking = true, CancellationToken token = default);

    /// <summary>
    /// 异步根据主键和导航属性获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="navigationPropertyPaths">导航属性路径</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    Task<TEntity?> GetWithNavigationPropertiesAsync(TKey keyValue,
        IEnumerable<Expression<Func<TEntity, dynamic>>>? navigationPropertyPaths = null, bool noTracking = true,
        CancellationToken token = default);

    /// <summary>
    /// 获取所有实体
    /// </summary>
    IQueryable<TEntity> GetAll(bool noTracking = true);

    /// <summary>
    /// 异步获取分页列表
    /// </summary>
    /// <param name="whereExpression">查询条件</param>
    /// <param name="pageIndex">当前页索引</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="includeProperties">包含的导航属性</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    Task<PagedResult<TEntity>> GetPagedListAsync(Expression<Func<TEntity, bool>> whereExpression, 
        int pageIndex, int pageSize, 
        IEnumerable<Expression<Func<TEntity, dynamic>>>? includeProperties = null, bool noTracking = true,
        CancellationToken token = default);
}
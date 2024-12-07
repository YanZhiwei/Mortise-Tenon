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
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> whereExpression, bool noTracking = true,
        CancellationToken token = default);

    /// <summary>
    /// 异步根据主键获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    Task<TEntity?> GetAsync(TKey keyValue, bool noTracking = true, CancellationToken token = default);

    /// <summary>
    /// 异步根据主键和导航属性获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="navigationPropertyPaths">导航属性路径</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    Task<TEntity?> GetWithNavigationPropertiesAsync(TKey keyValue,
        IEnumerable<Expression<Func<TEntity, dynamic>>> navigationPropertyPaths = null, bool noTracking = true,
        CancellationToken token = default);

    /// <summary>
    /// 异步根据主键和单个导航属性获取实体
    /// </summary>
    /// <param name="keyValue">主键值</param>
    /// <param name="navigationPropertyPath">导航属性路径</param>
    /// <param name="noTracking">是否不追踪</param>
    /// <param name="token">取消令牌</param>
    Task<TEntity?> GetAsync(TKey keyValue, Expression<Func<TEntity, dynamic>> navigationPropertyPath = null,
        bool noTracking = true, CancellationToken token = default);

    /// <summary>
    /// 获取所有实体
    /// </summary>
    IQueryable<TEntity> GetAll(bool noTracking = true);
}
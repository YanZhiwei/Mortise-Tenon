using System.Linq.Expressions;

namespace Tenon.Repository;

/// <summary>
/// 定义通用仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public interface IRepository<TEntity, in TKey> where TEntity : IEntity<TKey>
{
    /// <summary>
    /// 异步插入单个实体
    /// </summary>
    Task<int> InsertAsync(TEntity entity, CancellationToken token = default);
    
    /// <summary>
    /// 异步插入多个实体
    /// </summary>
    Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken token = default);
    
    /// <summary>
    /// 异步更新单个实体
    /// </summary>
    Task<int> UpdateAsync(TEntity entity, CancellationToken token = default);
    
    /// <summary>
    /// 异步更新多个实体
    /// </summary>
    Task<int> UpdateAsync(IEnumerable<TEntity> entities, CancellationToken token = default);
    
    /// <summary>
    /// 异步检查是否存在满足条件的实体
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken token = default);
    
    /// <summary>
    /// 异步获取满足条件的实体数量
    /// </summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken token = default);
    
    /// <summary>
    /// 异步删除单个实体
    /// </summary>
    Task<int> RemoveAsync(TEntity entity, CancellationToken token = default);
    
    /// <summary>
    /// 异步根据主键删除实体
    /// </summary>
    Task<int> RemoveAsync(TKey keyValue, CancellationToken token = default);
    
    /// <summary>
    /// 异步删除多个实体
    /// </summary>
    Task<int> RemoveAsync(IEnumerable<TEntity> entities, CancellationToken token = default);

    /// <summary>
    /// 异步更新实体的指定属性
    /// </summary>
    Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>>[] updatingExpressions,
        CancellationToken token = default);

    /// <summary>
    /// 异步获取所有实体
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// 异步获取满足条件的实体列表
    /// </summary>
    Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> whereExpression,
        CancellationToken token = default);

    /// <summary>
    /// 异步根据主键获取实体
    /// </summary>
    Task<TEntity?> GetAsync(TKey keyValue, CancellationToken token = default);

    /// <summary>
    /// 异步根据主键和导航属性路径获取实体
    /// </summary>
    Task<TEntity?> GetAsync(TKey keyValue,
        IEnumerable<Expression<Func<TEntity, dynamic>>>? navigationPropertyPaths = null,
        CancellationToken token = default);

    /// <summary>
    /// 异步根据主键和单个导航属性路径获取实体
    /// </summary>
    Task<TEntity?> GetAsync(TKey keyValue, Expression<Func<TEntity, dynamic>>? navigationPropertyPath = null,
        CancellationToken token = default);
}
namespace Tenon.Repository;

/// <summary>
/// 用户审计信息接口
/// </summary>
/// <typeparam name="TKey">用户主键类型</typeparam>
public interface IAuditable<TKey>
{
    /// <summary>
    /// 用户主键
    /// </summary>
    TKey UserId { get; set; }
} 
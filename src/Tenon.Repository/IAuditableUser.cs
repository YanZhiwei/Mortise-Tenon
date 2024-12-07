namespace Tenon.Repository;

/// <summary>
/// 可审计用户接口
/// </summary>
/// <typeparam name="TKey">用户主键类型</typeparam>
public interface IAuditableUser<TKey>
{
    /// <summary>
    /// 用户主键
    /// </summary>
    TKey User { get; set; }
}
namespace Tenon.Repository;

/// <summary>
/// 基础可审计接口，包含创建和更新的时间戳。
/// </summary>
public interface IBasicAuditable
{
    /// <summary>
    /// 获取或设置创建时间。
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 获取或设置更新时间。
    /// </summary>
    DateTimeOffset? UpdatedAt { get; set; }
}
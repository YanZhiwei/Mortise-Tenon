using Tenon.Repository.EfCore;

namespace Tenon.Repository.EfCore.Tests.Entities;

/// <summary>
/// 用于测试并发的实体
/// </summary>
public class ConcurrentEntity : EfEntity, IConcurrency
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 行版本
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

using Tenon.Repository.EfCore;

namespace Tenon.Repository.EfCore.Tests.Entities;

/// <summary>
/// 博客标签实体
/// </summary>
public class BlogTag : EfEntity
{
    /// <summary>
    /// 标签名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 标签描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 使用次数
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// 关联的博客
    /// </summary>
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
} 
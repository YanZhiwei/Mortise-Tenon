using Tenon.Repository.EfCore;

namespace Tenon.Repository.EfCore.Tests.Entities;

/// <summary>
/// 博客实体
/// </summary>
public class Blog : EfFullAuditableEntity
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime PublishTime { get; set; }

    /// <summary>
    /// 阅读次数
    /// </summary>
    public int ReadCount { get; set; }

    /// <summary>
    /// 点赞次数
    /// </summary>
    public int LikeCount { get; set; }

    /// <summary>
    /// 博客标签
    /// </summary>
    public virtual ICollection<BlogTag> Tags { get; set; } = new List<BlogTag>();

    /// <summary>
    /// 博客评论
    /// </summary>
    public virtual ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
} 
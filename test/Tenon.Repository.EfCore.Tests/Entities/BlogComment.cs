using Tenon.Repository.EfCore;

namespace Tenon.Repository.EfCore.Tests.Entities;

/// <summary>
/// 博客评论实体
/// </summary>
public class BlogComment : EfEntity
{
    /// <summary>
    /// 博客ID
    /// </summary>
    public long BlogId { get; set; }

    /// <summary>
    /// 评论内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 评论者
    /// </summary>
    public string Commenter { get; set; } = string.Empty;

    /// <summary>
    /// 评论时间
    /// </summary>
    public DateTime CommentTime { get; set; }

    /// <summary>
    /// 点赞次数
    /// </summary>
    public int LikeCount { get; set; }

    /// <summary>
    /// 父评论ID
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// 关联的博客
    /// </summary>
    public virtual Blog Blog { get; set; } = null!;

    /// <summary>
    /// 父评论
    /// </summary>
    public virtual BlogComment? Parent { get; set; }

    /// <summary>
    /// 子评论
    /// </summary>
    public virtual ICollection<BlogComment> Children { get; set; } = new List<BlogComment>();
} 
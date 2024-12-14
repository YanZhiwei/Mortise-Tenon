using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Tenon.Repository.EfCore.Tests.Entities;
using Tenon.Repository.EfCore.Tests.Interceptors;

namespace Tenon.Repository.EfCore.Tests;

/// <summary>
///     博客数据库上下文
/// </summary>
public class BlogDbContext : TenonDbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    /// <summary>
    ///     博客
    /// </summary>
    public DbSet<Blog> Blogs { get; set; } = null!;

    /// <summary>
    ///     博客标签
    /// </summary>
    public DbSet<BlogTag> BlogTags { get; set; } = null!;

    /// <summary>
    ///     博客评论
    /// </summary>
    public DbSet<BlogComment> BlogComments { get; set; } = null!;

    /// <summary>
    ///     并发实体
    /// </summary>
    public DbSet<ConcurrentEntity> ConcurrentEntities { get; set; } = null!;

    /// <summary>
    ///     获取实体所在程序集
    /// </summary>
    protected override Assembly EntityAssembly => typeof(BlogDbContext).Assembly;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.AddInterceptors(new BlogSoftDeleteInterceptor());

    }
}
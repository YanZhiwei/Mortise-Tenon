using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
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
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            optionsBuilder.UseSqlite(configuration.GetSection("Database:ConnectionString").Value);
        }

        base.OnConfiguring(optionsBuilder);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.AddInterceptors(new BlogSoftDeleteInterceptor());
    }
}

/// <summary>
///     BlogDbContext 的设计时工厂
/// </summary>
public class BlogDbContextFactory : IDesignTimeDbContextFactory<BlogDbContext>
{
    public BlogDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<BlogDbContext>();
        optionsBuilder.UseSqlite(configuration.GetSection("Database:ConnectionString").Value);

        return new BlogDbContext(optionsBuilder.Options);
    }
}
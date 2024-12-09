using Microsoft.EntityFrameworkCore;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests;

/// <summary>
/// 博客数据库上下文
/// </summary>
public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 博客集合
    /// </summary>
    public DbSet<Blog> Blogs { get; set; } = null!;

    /// <summary>
    /// 博客标签集合
    /// </summary>
    public DbSet<BlogTag> BlogTags { get; set; } = null!;

    /// <summary>
    /// 博客评论集合
    /// </summary>
    public DbSet<BlogComment> BlogComments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Blog>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Content).IsRequired();
            builder.Property(x => x.Author).IsRequired().HasMaxLength(50);
            builder.Property(x => x.PublishTime).IsRequired();
            
            builder.HasMany(x => x.Tags)
                .WithMany(x => x.Blogs)
                .UsingEntity(j => j.ToTable("BlogTagRelations"));

            builder.HasMany(x => x.Comments)
                .WithOne(x => x.Blog)
                .HasForeignKey(x => x.BlogId)
                .OnDelete(DeleteBehavior.Cascade);

            // 添加全局查询过滤器，过滤已删除的博客
            builder.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<BlogTag>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(200);
        });

        modelBuilder.Entity<BlogComment>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Content).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.Commenter).IsRequired().HasMaxLength(50);
            builder.Property(x => x.CommentTime).IsRequired();

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            // 添加全局查询过滤器，过滤已删除的评论
            builder.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    public override int SaveChanges()
    {
        HandleSoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void HandleSoftDelete()
    {
        var entries = ChangeTracker.Entries<BlogComment>()
            .Where(e => e.State != EntityState.Deleted && e.Entity.IsDeleted)
            .ToList();

        foreach (var entry in entries)
        {
            // 获取所有子评论（包括已删除的）
            var childComments = BlogComments
                .IgnoreQueryFilters()
                .Where(c => c.ParentId == entry.Entity.Id)
                .ToList();

            // 标记子评论为已删除
            foreach (var childComment in childComments)
            {
                childComment.IsDeleted = true;
                childComment.DeletedAt = DateTimeOffset.UtcNow;
                childComment.DeletedBy = entry.Entity.DeletedBy;
            }
        }
    }
} 
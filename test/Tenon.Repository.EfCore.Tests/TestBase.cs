using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests;

/// <summary>
///     测试基类
/// </summary>
public abstract class TestBase
{
    protected ILogger<EfRepository<BlogComment>> BlogCommentLogger;
    protected ILogger<EfRepository<Blog>> BlogLogger;
    protected ILogger<EfRepository<BlogTag>> BlogTagLogger;
    protected BlogDbContext DbContext { get; private set; } = null!;
    protected EfRepository<Blog> BlogEfRepo { get; private set; } = null!;
    protected EfRepository<BlogTag> BlogTagEfRepo { get; private set; } = null!;
    protected EfRepository<BlogComment> BlogCommentEfRepo { get; private set; } = null!;

    [TestInitialize]
    public virtual void Setup()
    {
        var services = new ServiceCollection();

        // 配置数据库
        services.AddDbContext<BlogDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // 配置日志
        services.AddLogging(builder => builder.AddConsole());

        // 注册DbContext作为通用DbContext
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<BlogDbContext>());

        // 注册仓储
        services.AddScoped<EfRepository<Blog>>();
        services.AddScoped<EfRepository<BlogTag>>();
        services.AddScoped<EfRepository<BlogComment>>();
        services.AddScoped<IRepository<Blog, long>>(sp => sp.GetRequiredService<EfRepository<Blog>>());
        services.AddScoped<IRepository<BlogTag, long>>(sp => sp.GetRequiredService<EfRepository<BlogTag>>());
        services.AddScoped<IRepository<BlogComment, long>>(sp => sp.GetRequiredService<EfRepository<BlogComment>>());

        var serviceProvider = services.BuildServiceProvider();
        InitializeServices(serviceProvider);
        InitializeTestData();
    }

    /// <summary>
    ///     初始化服务
    /// </summary>
    private void InitializeServices(IServiceProvider serviceProvider)
    {
        DbContext = serviceProvider.GetRequiredService<BlogDbContext>();
        BlogLogger = serviceProvider.GetRequiredService<ILogger<EfRepository<Blog>>>();
        BlogTagLogger = serviceProvider.GetRequiredService<ILogger<EfRepository<BlogTag>>>();
        BlogCommentLogger = serviceProvider.GetRequiredService<ILogger<EfRepository<BlogComment>>>();
        BlogEfRepo = serviceProvider.GetRequiredService<EfRepository<Blog>>();
        BlogTagEfRepo = serviceProvider.GetRequiredService<EfRepository<BlogTag>>();
        BlogCommentEfRepo = serviceProvider.GetRequiredService<EfRepository<BlogComment>>();
    }

    /// <summary>
    ///     初始化测试数据
    /// </summary>
    private void InitializeTestData()
    {
        var tags = InitializeBlogTags();
        var blogs = InitializeBlogs(tags);
        var comments = InitializeBlogComments(blogs);
        InitializeChildComments(blogs, comments);
    }

    /// <summary>
    ///     初始化博客标签
    /// </summary>
    private List<BlogTag> InitializeBlogTags()
    {
        var tags = new List<BlogTag>
        {
            new() {Name = "技术", Description = "技术相关文章"},
            new() {Name = "生活", Description = "生活随笔"},
            new() {Name = "编程", Description = "编程技巧"}
        };
        DbContext.BlogTags.AddRange(tags);
        DbContext.SaveChanges();
        return tags;
    }

    /// <summary>
    ///     初始化博客
    /// </summary>
    private List<Blog> InitializeBlogs(List<BlogTag> tags)
    {
        var blogs = new List<Blog>
        {
            new()
            {
                Title = "第一篇博客",
                Content = "这是第一篇博客的内容",
                Author = "张三",
                PublishTime = DateTime.Now.AddDays(-5),
                ReadCount = 100,
                LikeCount = 10,
                Tags = new List<BlogTag> {tags[0], tags[2]}
            },
            new()
            {
                Title = "第二篇博客",
                Content = "这是第二篇博客的内容",
                Author = "李四",
                PublishTime = DateTime.Now.AddDays(-3),
                ReadCount = 200,
                LikeCount = 20,
                Tags = new List<BlogTag> {tags[1]}
            },
            new()
            {
                Title = "第三篇博客",
                Content = "这是第三篇博客的内容",
                Author = "张三",
                PublishTime = DateTime.Now.AddDays(-1),
                ReadCount = 150,
                LikeCount = 15,
                Tags = new List<BlogTag> {tags[0], tags[1], tags[2]}
            }
        };
        DbContext.Blogs.AddRange(blogs);
        DbContext.SaveChanges();
        return blogs;
    }

    /// <summary>
    ///     初始化博客评论
    /// </summary>
    private List<BlogComment> InitializeBlogComments(List<Blog> blogs)
    {
        var comments = new List<BlogComment>
        {
            new()
            {
                BlogId = blogs[0].Id,
                Content = "很好的文章！",
                Commenter = "王五",
                CommentTime = DateTime.Now.AddDays(-4),
                LikeCount = 5
            },
            new()
            {
                BlogId = blogs[0].Id,
                Content = "学习了！",
                Commenter = "赵六",
                CommentTime = DateTime.Now.AddDays(-4).AddHours(2),
                LikeCount = 3
            },
            new()
            {
                BlogId = blogs[1].Id,
                Content = "写得不错！",
                Commenter = "王五",
                CommentTime = DateTime.Now.AddDays(-2),
                LikeCount = 4
            }
        };
        DbContext.BlogComments.AddRange(comments);
        DbContext.SaveChanges();
        return comments;
    }

    /// <summary>
    ///     初始化子评论
    /// </summary>
    private void InitializeChildComments(List<Blog> blogs, List<BlogComment> comments)
    {
        var childComments = new List<BlogComment>
        {
            new()
            {
                BlogId = blogs[0].Id,
                Content = "回复@王五：谢谢支持！",
                Commenter = "张三",
                CommentTime = DateTime.Now.AddDays(-4).AddHours(1),
                LikeCount = 2,
                ParentId = comments[0].Id
            },
            new()
            {
                BlogId = blogs[1].Id,
                Content = "回复@王五：感谢评论！",
                Commenter = "李四",
                CommentTime = DateTime.Now.AddDays(-2).AddHours(1),
                LikeCount = 1,
                ParentId = comments[2].Id
            }
        };
        DbContext.BlogComments.AddRange(childComments);
        DbContext.SaveChanges();
    }


    [TestCleanup]
    public virtual void Cleanup()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}
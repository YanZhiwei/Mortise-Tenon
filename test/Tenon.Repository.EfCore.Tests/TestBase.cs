using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Repository.EfCore.Tests.Entities;
using Microsoft.Extensions.Logging;
namespace Tenon.Repository.EfCore.Tests;

/// <summary>
/// 测试基类
/// </summary>
public abstract class TestBase
{
    protected ILogger<EfRepository<Blog>> BlogLogger;
    protected ILogger<EfRepository<BlogTag>> BlogTagLogger;
    protected ILogger<EfRepository<BlogComment>> BlogCommentLogger;
    protected BlogDbContext DbContext { get; private set; } = null!;
    protected EfRepository<Blog> BlogEfRepo { get; private set; } = null!;
    protected EfRepository<BlogTag> BlogTagEfRepo { get; private set; } = null!;
    protected EfRepository<BlogComment> BlogCommentEfRepo { get; private set; } = null!;

    [TestInitialize]
    public virtual void Setup()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var loggerFactory = LoggerFactory.Create(builder => // 创建日志工厂
        {
            builder.AddConsole(); // 可以根据需要添加其他日志提供程序
        });

        BlogLogger = loggerFactory.CreateLogger<EfRepository<Blog>>(); // 初始化日志
        BlogTagLogger = loggerFactory.CreateLogger<EfRepository<BlogTag>>(); // 初始化日志
        BlogCommentLogger = loggerFactory.CreateLogger<EfRepository<BlogComment>>(); // 初始化日志
        DbContext = new BlogDbContext(options);
        BlogEfRepo = new EfRepository<Blog>(DbContext, BlogLogger);
        BlogTagEfRepo = new EfRepository<BlogTag>(DbContext, BlogTagLogger);
        BlogCommentEfRepo = new EfRepository<BlogComment>(DbContext, BlogCommentLogger);

        // 初始化测试数据
        var tags = new List<BlogTag>
        {
            new BlogTag { Name = "技术", Description = "技术相关文章" },
            new BlogTag { Name = "生活", Description = "生活随笔" },
            new BlogTag { Name = "编程", Description = "编程技巧" }
        };
        DbContext.BlogTags.AddRange(tags);
        DbContext.SaveChanges();

        var blogs = new List<Blog>
        {
            new Blog
            {
                Title = "第一篇博客",
                Content = "这是第一篇博客的内容",
                Author = "张三",
                PublishTime = DateTime.Now.AddDays(-5),
                ReadCount = 100,
                LikeCount = 10,
                Tags = new List<BlogTag> { tags[0], tags[2] }
            },
            new Blog
            {
                Title = "第二篇博客",
                Content = "这是第二篇博客的内容",
                Author = "李四",
                PublishTime = DateTime.Now.AddDays(-3),
                ReadCount = 200,
                LikeCount = 20,
                Tags = new List<BlogTag> { tags[1] }
            },
            new Blog
            {
                Title = "第三篇博客",
                Content = "这是第三篇博客的内容",
                Author = "张三",
                PublishTime = DateTime.Now.AddDays(-1),
                ReadCount = 150,
                LikeCount = 15,
                Tags = new List<BlogTag> { tags[0], tags[1], tags[2] }
            }
        };
        DbContext.Blogs.AddRange(blogs);
        DbContext.SaveChanges();

        var comments = new List<BlogComment>
        {
            new BlogComment
            {
                BlogId = blogs[0].Id,
                Content = "很好的文章！",
                Commenter = "王五",
                CommentTime = DateTime.Now.AddDays(-4),
                LikeCount = 5
            },
            new BlogComment
            {
                BlogId = blogs[0].Id,
                Content = "学习了！",
                Commenter = "赵六",
                CommentTime = DateTime.Now.AddDays(-4).AddHours(2),
                LikeCount = 3
            },
            new BlogComment
            {
                BlogId = blogs[1].Id,
                Content = "写得不错！",
                Commenter = "王五",
                CommentTime = DateTime.Now.AddDays(-2),
                LikeCount = 4
            }
        };
        DbContext.BlogComments.AddRange(comments);

        // 添加子评论
        var childComments = new List<BlogComment>
        {
            new BlogComment
            {
                BlogId = blogs[0].Id,
                Content = "回复@王五：谢谢支持！",
                Commenter = "张三",
                CommentTime = DateTime.Now.AddDays(-4).AddHours(1),
                LikeCount = 2,
                ParentId = comments[0].Id
            },
            new BlogComment
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
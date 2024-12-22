using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Repository.EfCore.Extensions;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests;

/// <summary>
///     测试基类
/// </summary>
public abstract class TestBase
{
    private const string DbFileName = "test.db";
    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    public required ILogger<EfRepository<BlogComment>> BlogCommentLogger { get; set; }
    public required ILogger<EfRepository<Blog>> BlogLogger { get; set; }
    public required ILogger<EfRepository<BlogTag>> BlogTagLogger { get; set; }
    protected BlogDbContext DbContext { get; private set; } = null!;
    protected EfRepository<Blog> BlogEfRepo { get; private set; } = null!;
    protected EfRepository<BlogTag> BlogTagEfRepo { get; private set; } = null!;
    protected EfRepository<BlogComment> BlogCommentEfRepo { get; private set; } = null!;
    protected EfRepository<ConcurrentEntity> ConcurrentEfRepo { get; private set; } = null!;

    [TestInitialize]
    public virtual async Task Setup()
    {
        await CleanupDatabase();

        var services = new ServiceCollection();

        // 构建配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        // 配置日志
        services.AddLogging(builder =>
            builder.AddConsole()
                .SetMinimumLevel(LogLevel.Information));

        // 注册审计用户
        services.AddScoped<IAuditable<long>>(_ => new EfAuditable {UserId = 1});

        // 使用 AddEfCore 扩展方法注册仓储和 DbContext
        services.AddEfCore<BlogDbContext, TestUnitOfWork>(
            configuration.GetSection("Database"),
            options =>
            {
                options.UseSqlite($"Data Source={DbFileName}");
                options.EnableSensitiveDataLogging();
            }
        );

        var serviceProvider = services.BuildServiceProvider();
        await InitializeServices(serviceProvider);
    }

    [TestCleanup]
    public virtual async Task Cleanup()
    {
        if (DbContext != null)
        {
            await CleanupDatabase();
            await DbContext.DisposeAsync();
        }
    }

    /// <summary>
    ///     初始化服务
    /// </summary>
    private async Task InitializeServices(IServiceProvider serviceProvider)
    {
        DbContext = serviceProvider.GetRequiredService<BlogDbContext>();
        BlogLogger = serviceProvider.GetRequiredService<ILogger<EfRepository<Blog>>>();
        BlogTagLogger = serviceProvider.GetRequiredService<ILogger<EfRepository<BlogTag>>>();
        BlogCommentLogger = serviceProvider.GetRequiredService<ILogger<EfRepository<BlogComment>>>();
        BlogEfRepo = serviceProvider.GetRequiredService<EfRepository<Blog>>();
        BlogTagEfRepo = serviceProvider.GetRequiredService<EfRepository<BlogTag>>();
        BlogCommentEfRepo = serviceProvider.GetRequiredService<EfRepository<BlogComment>>();
        ConcurrentEfRepo = serviceProvider.GetRequiredService<EfRepository<ConcurrentEntity>>();

        lock (_lock)
        {
            if (!_databaseInitialized)
            {
                // 删除现有数据库
                if (File.Exists(DbFileName))
                {
                    try
                    {
                        File.Delete(DbFileName);
                    }
                    catch (IOException)
                    {
                        // 忽略文件被占用的异常
                    }
                }

                // 确保数据库已创建
                DbContext.Database.EnsureCreated();
                _databaseInitialized = true;
            }
        }

        // 清理所有数据
        await CleanupDatabaseData();
    }

    /// <summary>
    ///     清理数据库
    /// </summary>
    private async Task CleanupDatabase()
    {
        if (DbContext != null)
        {
            try
            {
                await CleanupDatabaseData();
            }
            finally
            {
                DbContext.ChangeTracker.Clear();
            }
        }
    }

    /// <summary>
    ///     清理数据库数据
    /// </summary>
    private async Task CleanupDatabaseData()
    {
        // 使用事务确保原子性
        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            // 禁用外键约束
            await DbContext.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");

            // 删除所有表数据
            await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM BlogBlogTag");
            await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM BlogComments");
            await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM BlogTags");
            await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Blogs");
            await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM ConcurrentEntities");

            // 启用外键约束
            await DbContext.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");

            // 提交事务
            await transaction.CommitAsync();
        }
        catch
        {
            // 回滚事务
            await transaction.RollbackAsync();
            throw;
        }
    }
}
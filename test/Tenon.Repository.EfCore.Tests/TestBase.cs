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
    
    protected required ILogger<EfRepository<BlogComment>> BlogCommentLogger { get; set; }
    protected required ILogger<EfRepository<Blog>> BlogLogger { get; set; }
    protected required ILogger<EfRepository<BlogTag>> BlogTagLogger { get; set; }
    protected BlogDbContext DbContext { get; private set; } = null!;
    protected EfRepository<Blog> BlogEfRepo { get; private set; } = null!;
    protected EfRepository<BlogTag> BlogTagEfRepo { get; private set; } = null!;
    protected EfRepository<BlogComment> BlogCommentEfRepo { get; private set; } = null!;
    protected EfRepository<ConcurrentEntity> ConcurrentEfRepo { get; private set; } = null!;

    [TestInitialize]
    public virtual async Task Setup()
    {
        // 删除可能存在的旧数据库文件
        if (File.Exists(DbFileName))
        {
            File.Delete(DbFileName);
        }

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

        // 注册 EfAuditableUser
        services.AddScoped<EfUserAuditInfo>(_ => new EfUserAuditInfo { UserId = 1 });

        // 使用 AddEfCore 扩展方法注册仓储和 DbContext
        services.AddEfCore<BlogDbContext, TestUnitOfWork>(
            configuration.GetSection("Database"),
            options => options.UseSqlite(configuration.GetSection("Database:ConnectionString").Value)
        );

        var serviceProvider = services.BuildServiceProvider();
        await InitializeServices(serviceProvider);
    }

    [TestCleanup]
    public virtual void Cleanup()
    {
        DbContext.Dispose();
        
        // 清理数据库文件
        if (File.Exists(DbFileName))
        {
            File.Delete(DbFileName);
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

        // 确保数据库已创建
        await DbContext.Database.EnsureCreatedAsync();
    }
}
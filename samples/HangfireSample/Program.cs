using Hangfire.Storage.SQLite;
using HangfireSample.Caching;
using HangfireSample.Services;
using Scalar.AspNetCore;
using Tenon.Hangfire.Extensions.Caching;
using Tenon.Hangfire.Extensions.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 添加认证
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

// 添加 OpenAPI
builder.Services.AddOpenApi();

// 添加示例任务服务
builder.Services.AddScoped<SampleJobService>();

// 注册 Hangfire 缓存提供程序
builder.Services.AddSingleton<IHangfireCacheProvider, HangfireMemoryCacheProvider>();

// 添加 Hangfire 服务
builder.Services.AddHangfireServices(
    // 配置节
    builder.Configuration.GetSection("Hangfire"),
    
    // 配置存储
    configureStorage: config =>
    {
        // 配置 SQLite 存储
        var storageOptions = new SQLiteStorageOptions
        {
            // 基础配置
            Prefix = "hangfire", // 表前缀
            QueuePollInterval = TimeSpan.FromSeconds(15), // 队列轮询间隔
            InvisibilityTimeout = TimeSpan.FromMinutes(30), // 任务隐藏超时
            DistributedLockLifetime = TimeSpan.FromSeconds(30), // 分布式锁超时

            // 维护配置
            JobExpirationCheckInterval = TimeSpan.FromHours(1), // 过期任务检查间隔
            CountersAggregateInterval = TimeSpan.FromMinutes(5), // 计数器聚合间隔

            // 性能配置
            PoolSize = Environment.ProcessorCount * 2, // 连接池大小
            JournalMode = SQLiteStorageOptions.JournalModes.WAL, // WAL模式提高并发性能
            AutoVacuumSelected = SQLiteStorageOptions.AutoVacuum.INCREMENTAL // 增量式自动清理
        };

        config.UseSQLiteStorage(
            builder.Configuration.GetConnectionString("HangfireConnection"),
            storageOptions);
    },
    
    // 配置服务器选项
    configureServer: options =>
    {
        // 方式一：直接配置
        if (builder.Environment.IsDevelopment())
        {
            // 开发环境配置
            options.WorkerCount = 5; // 减少工作线程数
            options.Queues = new[] { "development", "default" }; // 开发队列优先
            options.ServerTimeout = TimeSpan.FromMinutes(2); // 更短的超时
            options.ServerName = $"Dev.{Environment.MachineName}"; // 开发服务器名称
        }
        else
        {
            // 生产环境配置
            options.WorkerCount = Environment.ProcessorCount * 4; // 更多工作线程
            options.Queues = new[] { "critical", "default", "low" }; // 生产队列优先级
            options.ServerTimeout = TimeSpan.FromMinutes(10); // 更长的超时
            options.ServerName = $"Prod.{Environment.MachineName}"; // 生产服务器名称
        }

        // 方式二：从配置文件加载
        var serverSection = builder.Configuration.GetSection("Hangfire:Server");
        if (serverSection.Exists())
        {
            var workerCount = serverSection.GetValue<int?>("WorkerCount");
            if (workerCount.HasValue)
            {
                options.WorkerCount = workerCount.Value;
            }

            var queues = serverSection.GetSection("Queues").Get<string[]>();
            if (queues?.Length > 0)
            {
                options.Queues = queues;
            }

            var serverTimeout = serverSection.GetValue<int?>("ServerTimeoutMinutes");
            if (serverTimeout.HasValue)
            {
                options.ServerTimeout = TimeSpan.FromMinutes(serverTimeout.Value);
            }
        }

        // 方式三：从环境变量获取
        var envWorkerCount = Environment.GetEnvironmentVariable("HANGFIRE_WORKER_COUNT");
        if (!string.IsNullOrEmpty(envWorkerCount) && int.TryParse(envWorkerCount, out var count))
        {
            options.WorkerCount = count;
        }

        // 方式四：动态计算
        options.WorkerCount = Math.Min(
            Environment.ProcessorCount * 5, // 最大线程数
            Math.Max(5, Environment.ProcessorCount * 2) // 最小线程数
        );
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// 使用 Hangfire
app.UseHangfire();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
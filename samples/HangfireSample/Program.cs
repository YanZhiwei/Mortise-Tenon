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
    builder.Configuration.GetSection("Hangfire"),
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
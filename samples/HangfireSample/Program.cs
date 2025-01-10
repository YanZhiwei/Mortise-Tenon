using Hangfire;
using Hangfire.Storage.SQLite;
using HangfireSample.Services;
using Tenon.Hangfire.Extensions.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加示例任务服务
builder.Services.AddScoped<SampleJobService>();

// 添加 Hangfire 服务
builder.Services.AddHangfireServices(builder.Configuration);

// 确保数据库目录存在
var dbDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
if (!Directory.Exists(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

// 配置 SQLite 存储选项
var storageOptions = new SQLiteStorageOptions
{
    QueuePollInterval = TimeSpan.FromSeconds(15) // 轮询间隔
};

// 配置连接字符串
var connectionString = builder.Configuration.GetConnectionString("HangfireConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("未配置 Hangfire 数据库连接字符串");
}

// 添加 Hangfire 服务
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(connectionString, storageOptions));

// 配置 Hangfire 服务器选项
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2; // 工作线程数
    options.Queues = new[] { "default", "critical" }; // 任务队列
    options.ServerTimeout = TimeSpan.FromMinutes(5); // 服务器超时
    options.ShutdownTimeout = TimeSpan.FromMinutes(2); // 关闭超时
    options.ServerName = $"Hangfire.Server.{Environment.MachineName}"; // 服务器名称
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 使用 Hangfire 仪表板
app.UseHangfire(app.Configuration.GetSection("Hangfire"));

app.Run(); 
using Hangfire;
using Hangfire.Storage.SQLite;
using HangfireSample.Services;
using Tenon.Hangfire.Extensions.Configuration;
using Tenon.Hangfire.Extensions.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加示例任务服务
builder.Services.AddScoped<SampleJobService>();

// 配置 Hangfire 选项
var hangfireSection = builder.Configuration.GetSection("Hangfire");
builder.Services.Configure<HangfireOptions>(hangfireSection);

// 配置认证选项
var authSection = hangfireSection.GetSection("Authentication");
builder.Services.Configure<AuthenticationOptions>(authSection);

// 获取认证选项的实例
var authOptions = authSection.Get<AuthenticationOptions>();
builder.Services.AddSingleton(authOptions ?? new AuthenticationOptions());

// 添加 Hangfire 服务
builder.Services.AddHangfireServices(builder.Configuration);

// 配置 SQLite 存储选项
var storageOptions = new SQLiteStorageOptions
{
    QueuePollInterval = TimeSpan.FromSeconds(15) // 轮询间隔
};

// 添加 Hangfire 服务
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(builder.Configuration.GetConnectionString("HangfireConnection"), storageOptions));

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
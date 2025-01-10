using Hangfire;
using HangfireSample.Services;
using Microsoft.AspNetCore.Mvc;

namespace HangfireSample.Controllers;

/// <summary>
///     任务控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JobController : ControllerBase
{
    private readonly ILogger<JobController> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly SampleJobService _sampleJobService;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="backgroundJobClient">后台任务客户端</param>
    /// <param name="recurringJobManager">定时任务管理器</param>
    /// <param name="sampleJobService">示例任务服务</param>
    public JobController(
        ILogger<JobController> logger,
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        SampleJobService sampleJobService)
    {
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _sampleJobService = sampleJobService;
    }

    /// <summary>
    ///     创建即时任务
    /// </summary>
    /// <returns>任务ID</returns>
    [HttpPost("fire-and-forget")]
    public IActionResult CreateFireAndForgetJob()
    {
        var jobId = _backgroundJobClient.Enqueue(() => _sampleJobService.ExecuteSimpleJob("FireAndForget"));
        return Ok(new { JobId = jobId });
    }

    /// <summary>
    ///     创建延迟任务
    /// </summary>
    /// <param name="delayInSeconds">延迟秒数</param>
    /// <returns>任务ID</returns>
    [HttpPost("delayed")]
    public IActionResult CreateDelayedJob([FromQuery] int delayInSeconds = 30)
    {
        var jobId = _backgroundJobClient.Schedule(
            () => _sampleJobService.ExecuteSimpleJob("Delayed"),
            TimeSpan.FromSeconds(delayInSeconds));
        return Ok(new { JobId = jobId });
    }

    /// <summary>
    ///     创建定时任务
    /// </summary>
    /// <param name="cronExpression">Cron 表达式</param>
    /// <returns>操作结果</returns>
    [HttpPost("recurring")]
    public IActionResult CreateRecurringJob([FromQuery] string cronExpression = "*/5 * * * *")
    {
        _recurringJobManager.AddOrUpdate(
            "sample-recurring-job",
            () => _sampleJobService.ExecuteSimpleJob("Recurring"),
            cronExpression);
        return Ok(new { Message = "Recurring job created" });
    }

    /// <summary>
    ///     创建连续任务
    /// </summary>
    /// <returns>任务ID</returns>
    [HttpPost("continuation")]
    public IActionResult CreateContinuationJob()
    {
        var jobId = _backgroundJobClient.Enqueue(() => _sampleJobService.ExecuteSimpleJob("First"));
        _backgroundJobClient.ContinueJobWith(jobId,
            () => _sampleJobService.ExecuteSimpleJob("Continuation"));
        return Ok(new { JobId = jobId });
    }

    /// <summary>
    ///     创建带参数的任务
    /// </summary>
    /// <param name="parameters">参数</param>
    /// <returns>任务ID</returns>
    [HttpPost("parameterized")]
    public IActionResult CreateParameterizedJob([FromBody] Dictionary<string, string> parameters)
    {
        var jobId = _backgroundJobClient.Enqueue(
            () => _sampleJobService.ExecuteParameterizedJob("Parameterized", parameters));
        return Ok(new { JobId = jobId });
    }

    /// <summary>
    ///     创建长时间运行的任务
    /// </summary>
    /// <param name="duration">持续时间（秒）</param>
    /// <returns>任务ID</returns>
    [HttpPost("long-running")]
    public IActionResult CreateLongRunningJob([FromQuery] int duration = 60)
    {
        var jobId = _backgroundJobClient.Enqueue(
            () => _sampleJobService.ExecuteLongRunningJob("LongRunning", duration));
        return Ok(new { JobId = jobId });
    }

    /// <summary>
    ///     创建可能失败的任务
    /// </summary>
    /// <param name="shouldFail">是否应该失败</param>
    /// <returns>任务ID</returns>
    [HttpPost("failable")]
    public IActionResult CreateFailableJob([FromQuery] bool shouldFail = true)
    {
        var jobId = _backgroundJobClient.Enqueue(
            () => _sampleJobService.ExecuteFailableJob("Failable", shouldFail));
        return Ok(new { JobId = jobId });
    }
} 
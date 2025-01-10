namespace HangfireSample.Services;

/// <summary>
///     示例任务服务
/// </summary>
public class SampleJobService
{
    private readonly ILogger<SampleJobService> _logger;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public SampleJobService(ILogger<SampleJobService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     执行简单任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    public void ExecuteSimpleJob(string jobName)
    {
        _logger.LogInformation("执行简单任务 {JobName} 开始: {Time}", jobName, DateTime.Now);
        // 模拟任务执行
        Thread.Sleep(5000);
        _logger.LogInformation("执行简单任务 {JobName} 完成: {Time}", jobName, DateTime.Now);
    }

    /// <summary>
    ///     执行带参数的任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="parameters">参数</param>
    public void ExecuteParameterizedJob(string jobName, Dictionary<string, string> parameters)
    {
        _logger.LogInformation("执行带参数任务 {JobName} 开始: {Time}", jobName, DateTime.Now);
        foreach (var param in parameters)
        {
            _logger.LogInformation("参数 {Key}: {Value}", param.Key, param.Value);
        }
        // 模拟任务执行
        Thread.Sleep(5000);
        _logger.LogInformation("执行带参数任务 {JobName} 完成: {Time}", jobName, DateTime.Now);
    }

    /// <summary>
    ///     执行长时间运行的任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="duration">持续时间（秒）</param>
    public void ExecuteLongRunningJob(string jobName, int duration)
    {
        _logger.LogInformation("执行长时间运行任务 {JobName} 开始: {Time}", jobName, DateTime.Now);
        var startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds < duration)
        {
            _logger.LogInformation("任务 {JobName} 运行中: {Progress}%",
                jobName, ((DateTime.Now - startTime).TotalSeconds / duration * 100).ToString("F2"));
            Thread.Sleep(1000);
        }
        _logger.LogInformation("执行长时间运行任务 {JobName} 完成: {Time}", jobName, DateTime.Now);
    }

    /// <summary>
    ///     执行可能失败的任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="shouldFail">是否应该失败</param>
    public void ExecuteFailableJob(string jobName, bool shouldFail)
    {
        _logger.LogInformation("执行可能失败任务 {JobName} 开始: {Time}", jobName, DateTime.Now);
        if (shouldFail)
        {
            throw new Exception($"任务 {jobName} 执行失败");
        }
        // 模拟任务执行
        Thread.Sleep(5000);
        _logger.LogInformation("执行可能失败任务 {JobName} 完成: {Time}", jobName, DateTime.Now);
    }
} 
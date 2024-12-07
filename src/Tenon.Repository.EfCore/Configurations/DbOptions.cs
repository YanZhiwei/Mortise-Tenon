namespace Tenon.Repository.EfCore.Configurations;

/// <summary>
///     数据库配置选项
/// </summary>
public class DbOptions
{
    /// <summary>
    ///     连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
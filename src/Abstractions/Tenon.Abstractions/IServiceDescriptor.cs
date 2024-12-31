namespace Tenon.Abstractions;

/// <summary>
/// 服务描述符接口
/// </summary>
/// <remarks>
/// 定义服务的基本信息，包括服务标识、名称、版本和描述等。
/// 用于服务注册、发现和管理。
/// </remarks>
public interface IServiceDescriptor
{
    /// <summary>
    /// 获取服务唯一标识符
    /// </summary>
    /// <value>服务的全局唯一标识符</value>
    string Id { get; }

    /// <summary>
    /// 获取服务名称
    /// </summary>
    /// <value>服务的友好显示名称</value>
    string ServiceName { get; }

    /// <summary>
    /// 获取服务版本号
    /// </summary>
    /// <value>遵循语义化版本规范的版本号（如：1.0.0）</value>
    /// <remarks>
    /// 版本号应遵循语义化版本规范 (SemVer)
    /// 格式：主版本号.次版本号.修订号
    /// </remarks>
    string Version { get; }

    /// <summary>
    /// 获取服务描述信息
    /// </summary>
    /// <value>服务的详细描述文本</value>
    string Description { get; }
}
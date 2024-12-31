namespace Tenon.Abstractions;

/// <summary>
/// WebAPI 服务描述符接口
/// </summary>
/// <remarks>
/// 用于描述 WebAPI 服务的配置信息，包括跨域策略等
/// </remarks>
public interface IWebApiServiceDescriptor : IServiceDescriptor
{
    /// <summary>
    /// 获取 CORS 策略名称
    /// </summary>
    /// <value>CORS 策略的唯一标识符</value>
    string CorsPolicy { get; }
}
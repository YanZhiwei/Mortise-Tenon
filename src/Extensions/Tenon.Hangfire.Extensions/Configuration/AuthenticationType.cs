namespace Tenon.Hangfire.Extensions.Configuration;

/// <summary>
///     认证类型
/// </summary>
public enum AuthenticationType
{
    /// <summary>
    ///     基本认证
    /// </summary>
    Basic,

    /// <summary>
    ///     JWT 认证
    /// </summary>
    JWT,

    /// <summary>
    ///     OAuth2 认证
    /// </summary>
    OAuth2
} 
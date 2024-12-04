using System.Text;

namespace Tenon.AspNetCore.Configuration;

/// <summary>
/// JWT（JSON Web Token）配置选项类，用于定制和管理JWT令牌的各种安全和验证参数
/// </summary>
/// <remarks>
/// 此类提供了全面的JWT令牌配置能力，支持细粒度的安全性和验证控制
/// </remarks>
/// <example>
/// 配置示例：
/// <code>
/// var jwtOptions = new JwtOptions
/// {
///     ValidateIssuer = true,
///     ValidIssuer = "MyCompany",
///     SymmetricSecurityKey = "MySecretKeyForSigningTokens",
///     ValidateAudience = true,
///     ValidAudience = "MyApplication",
///     Expire = 60, // 访问令牌有效期60分钟
///     RefreshTokenExpire = 7 * 24 * 60, // 刷新令牌有效期7天
///     ClockSkew = 0 // 时间偏差容错
/// };
/// </code>
/// </example>
public class JwtOptions
{
    /// <summary>
    /// 令牌编码方式，默认使用UTF-8编码
    /// </summary>
    /// <value>用于对令牌进行编码的字符编码</value>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// 是否验证令牌的颁发者（Issuer）
    /// </summary>
    /// <value>
    /// true：启用颁发者验证
    /// false：禁用颁发者验证
    /// </value>
    /// <remarks>
    /// 启用后，令牌的颁发者必须与配置的 <see cref="ValidIssuer"/> 匹配
    /// </remarks>
    public bool ValidateIssuer { get; set; } = default;

    /// <summary>
    /// 有效的令牌颁发者（Issuer）
    /// </summary>
    /// <value>令牌的授权颁发机构名称</value>
    /// <example>
    /// 可以是公司名称、应用程序名称或服务名称，如 "MyCompany.AuthService"
    /// </example>
    public string ValidIssuer { get; set; } = string.Empty;

    /// <summary>
    /// 是否验证签名密钥
    /// </summary>
    /// <value>
    /// true：启用签名密钥验证
    /// false：禁用签名密钥验证
    /// </value>
    /// <remarks>
    /// 启用后，令牌的签名将使用 <see cref="SymmetricSecurityKey"/> 进行严格验证
    /// </remarks>
    public bool ValidateIssuerSigningKey { get; set; } = default;

    /// <summary>
    /// 对称安全密钥的字符串表示
    /// </summary>
    /// <value>用于签名和验证令牌的安全密钥</value>
    /// <remarks>
    /// 建议使用强随机生成的复杂密钥，长度至少32个字符
    /// </remarks>
    /// <example>
    /// "MyVeryLongAndComplexSecurityKeyThatShouldBeKeptSecret123!@#"
    /// </example>
    public string SymmetricSecurityKey { get; set; } = string.Empty;

    /// <summary>
    /// 签名密钥（备用属性）
    /// </summary>
    /// <value>可替代 <see cref="SymmetricSecurityKey"/> 的签名密钥</value>
    public string IssuerSigningKey { get; set; } = string.Empty;

    /// <summary>
    /// 是否验证令牌的受众（Audience）
    /// </summary>
    /// <value>
    /// true：启用受众验证
    /// false：禁用受众验证
    /// </value>
    /// <remarks>
    /// 启用后，令牌的受众必须与配置的 <see cref="ValidAudience"/> 匹配
    /// </remarks>
    public bool ValidateAudience { get; set; } = default!;

    /// <summary>
    /// 有效的令牌受众（Audience）
    /// </summary>
    /// <value>令牌的目标接收方</value>
    /// <example>
    /// 可以是应用程序名称或服务名称，如 "WebManager"、"MobileApp"
    /// </example>
    public string ValidAudience { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌的受众
    /// </summary>
    /// <value>专门用于刷新令牌的受众标识</value>
    /// <remarks>
    /// 可以与 <see cref="ValidAudience"/> 不同，提供更精细的访问控制
    /// </remarks>
    public string RefreshTokenAudience { get; set; } = string.Empty;

    /// <summary>
    /// 是否验证令牌的生命周期
    /// </summary>
    /// <value>
    /// true：启用令牌过期时间验证
    /// false：禁用令牌过期时间验证
    /// </value>
    /// <remarks>
    /// 启用后，系统将检查令牌是否在有效期内
    /// </remarks>
    public bool ValidateLifetime { get; set; } = default!;

    /// <summary>
    /// 是否要求令牌必须包含过期时间
    /// </summary>
    /// <value>
    /// true：强制要求令牌包含过期时间
    /// false：允许不包含过期时间的令牌
    /// </value>
    public bool RequireExpirationTime { get; set; }

    /// <summary>
    /// 时间偏差容错值（秒）
    /// </summary>
    /// <value>允许的服务器时间误差范围</value>
    /// <remarks>
    /// 用于处理服务器之间可能存在的时间微小差异
    /// 默认值为0，表示严格匹配
    /// </remarks>
    /// <example>
    /// 设置为5表示允许5秒的时间误差
    /// </example>
    public int ClockSkew { get; set; } = default;

    /// <summary>
    /// 访问令牌的有效期（分钟）
    /// </summary>
    /// <value>访问令牌从创建到过期的时间长度</value>
    /// <remarks>
    /// 建议根据安全性需求设置合理的过期时间
    /// 过短可能影响用户体验，过长可能增加安全风险
    /// </remarks>
    /// <example>
    /// 60：表示访问令牌有效期为1小时
    /// 120：表示访问令牌有效期为2小时
    /// </example>
    public int Expire { get; set; } = default;

    /// <summary>
    /// 刷新令牌的有效期（分钟）
    /// </summary>
    /// <value>刷新令牌从创建到过期的时间长度</value>
    /// <remarks>
    /// 通常设置为比访问令牌更长的时间，允许用户在较长时间内无需重新登录
    /// </remarks>
    /// <example>
    /// 7 * 24 * 60：表示刷新令牌有效期为7天
    /// 30 * 24 * 60：表示刷新令牌有效期为30天
    /// </example>
    public decimal RefreshTokenExpire { get; set; } = default;
}
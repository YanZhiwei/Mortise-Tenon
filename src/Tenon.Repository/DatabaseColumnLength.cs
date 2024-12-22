namespace Tenon.Repository;

/// <summary>
///     数据库列长度常量定义
/// </summary>
public static class DatabaseColumnLength
{
    #region 基础文本长度
    /// <summary>
    ///     极短文本，如单字符标识等
    /// </summary>
    public const int TinyText = 16;

    /// <summary>
    ///     短文本，如代码、简短名称等
    /// </summary>
    public const int ShortText = 32;

    /// <summary>
    ///     普通文本，如标题、名称等
    /// </summary>
    public const int NormalText = 64;

    /// <summary>
    ///     长文本，如描述、备注等
    /// </summary>
    public const int LongText = 256;

    /// <summary>
    ///     扩展文本，如内容等
    /// </summary>
    public const int ExtendedText = 1024;
    #endregion

    #region 系统与安全
    /// <summary>
    ///     用户名
    /// </summary>
    public const int UserNameLength = 32;

    /// <summary>
    ///     密码Hash
    /// </summary>
    public const int PasswordHashLength = 128;

    /// <summary>
    ///     角色名称
    /// </summary>
    public const int RoleNameLength = 32;

    /// <summary>
    ///     权限编码
    /// </summary>
    public const int PermissionCodeLength = 128;

    /// <summary>
    ///     Token长度
    /// </summary>
    public const int TokenLength = 512;

    /// <summary>
    ///     加密密钥长度
    /// </summary>
    public const int SecretKeyLength = 256;

    /// <summary>
    ///     设备标识符
    /// </summary>
    public const int DeviceIdLength = 64;
    #endregion

    #region 联系信息
    /// <summary>
    ///     邮箱地址（RFC 5321标准）
    /// </summary>
    public const int EmailLength = 254;

    /// <summary>
    ///     手机号码
    /// </summary>
    public const int PhoneNumberLength = 32;

    /// <summary>
    ///     固定电话（含国际区号）
    /// </summary>
    public const int TelephoneLength = 32;

    /// <summary>
    ///     传真号码
    /// </summary>
    public const int FaxLength = 32;

    /// <summary>
    ///     地址信息
    /// </summary>
    public const int AddressLength = 512;

    /// <summary>
    ///     邮政编码
    /// </summary>
    public const int PostalCodeLength = 16;
    #endregion

    #region 标识与编码
    /// <summary>
    ///     枚举字符串，用于存储枚举值
    /// </summary>
    public const int EnumLength = 20;

    /// <summary>
    ///     GUID字符串
    /// </summary>
    public const int GuidLength = 36;

    /// <summary>
    ///     货币代码（ISO 4217）
    /// </summary>
    public const int CurrencyCodeLength = 3;

    /// <summary>
    ///     语言代码（ISO 639）
    /// </summary>
    public const int LanguageCodeLength = 8;

    /// <summary>
    ///     国家/地区代码（ISO 3166）
    /// </summary>
    public const int CountryCodeLength = 3;

    /// <summary>
    ///     时区标识符
    /// </summary>
    public const int TimeZoneLength = 32;

    /// <summary>
    ///     业务编号
    /// </summary>
    public const int BusinessCodeLength = 32;

    /// <summary>
    ///     订单编号
    /// </summary>
    public const int OrderNumberLength = 32;

    /// <summary>
    ///     商品编号
    /// </summary>
    public const int ProductCodeLength = 32;
    #endregion

    #region 网络与路径
    /// <summary>
    ///     IP地址（考虑IPv6）
    /// </summary>
    public const int IpAddressLength = 64;

    /// <summary>
    ///     MAC地址
    /// </summary>
    public const int MacAddressLength = 17;

    /// <summary>
    ///     文件路径（考虑跨平台兼容性）
    /// </summary>
    public const int FilePathLength = 2048;

    /// <summary>
    ///     URL地址（考虑现代浏览器标准）
    /// </summary>
    public const int UrlLength = 2048;

    /// <summary>
    ///     域名
    /// </summary>
    public const int DomainLength = 256;
    #endregion

    #region 数据与配置
    /// <summary>
    ///     JSON数据
    /// </summary>
    public const int JsonLength = 4000;

    /// <summary>
    ///     XML数据
    /// </summary>
    public const int XmlLength = 4000;

    /// <summary>
    ///     配置键
    /// </summary>
    public const int ConfigKeyLength = 128;

    /// <summary>
    ///     配置值
    /// </summary>
    public const int ConfigValueLength = 4000;
    #endregion

    #region 文件与媒体
    /// <summary>
    ///     文件名
    /// </summary>
    public const int FileNameLength = 256;

    /// <summary>
    ///     文件扩展名
    /// </summary>
    public const int FileExtensionLength = 32;

    /// <summary>
    ///     MIME类型
    /// </summary>
    public const int MimeTypeLength = 128;

    /// <summary>
    ///     图片尺寸
    /// </summary>
    public const int ImageSizeLength = 16;

    /// <summary>
    ///     颜色代码
    /// </summary>
    public const int ColorCodeLength = 32;
    #endregion
} 
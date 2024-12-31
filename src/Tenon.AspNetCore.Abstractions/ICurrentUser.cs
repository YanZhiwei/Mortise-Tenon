namespace Tenon.AspNetCore.Abstractions;

/// <summary>
/// 表示当前已认证的用户信息
/// </summary>
/// <typeparam name="TKey">用户标识的类型</typeparam>
/// <remarks>
/// 此接口提供对当前已认证用户的基本信息访问，包括用户标识、角色、权限等。
/// 通常用于在 Web API 应用程序中获取当前登录用户的上下文信息。
/// </remarks>
public interface ICurrentUser<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 获取当前用户的唯一标识
    /// </summary>
    /// <remarks>
    /// 用户未登录时将抛出 ApplicationException 异常
    /// </remarks>
    /// <exception cref="ApplicationException">当用户上下文不可用时抛出</exception>
    TKey UserId { get; }

    /// <summary>
    /// 获取当前用户的登录名
    /// </summary>
    /// <remarks>
    /// 用户未登录时将抛出 ApplicationException 异常
    /// </remarks>
    /// <exception cref="ApplicationException">当用户上下文不可用时抛出</exception>
    string UserName { get; }

    /// <summary>
    /// 获取当前用户的真实姓名
    /// </summary>
    /// <remarks>
    /// 如果未设置，将返回空字符串
    /// </remarks>
    string RealName { get; }

    /// <summary>
    /// 获取当前用户的电子邮箱地址
    /// </summary>
    /// <remarks>
    /// 如果未设置，将返回空字符串
    /// </remarks>
    string Email { get; }

    /// <summary>
    /// 获取一个值，该值指示用户的电子邮箱是否已确认
    /// </summary>
    bool EmailConfirmed { get; }

    /// <summary>
    /// 获取当前用户的手机号码
    /// </summary>
    /// <remarks>
    /// 如果未设置，将返回空字符串
    /// </remarks>
    string PhoneNumber { get; }

    /// <summary>
    /// 获取一个值，该值指示用户的手机号是否已确认
    /// </summary>
    bool PhoneNumberConfirmed { get; }

    /// <summary>
    /// 获取当前用户所属的角色列表
    /// </summary>
    /// <remarks>
    /// 如果用户没有任何角色，将返回空列表
    /// </remarks>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// 获取当前用户所属的租户标识
    /// </summary>
    /// <remarks>
    /// 仅在多租户系统中使用。如果系统不支持多租户或用户不属于任何租户，将返回 null
    /// </remarks>
    long? TenantId { get; }

    /// <summary>
    /// 获取一个值，该值指示当前用户是否已通过身份验证
    /// </summary>
    /// <remarks>
    /// 返回 true 表示用户已登录，false 表示用户未登录或登录已过期
    /// </remarks>
    bool IsAuthenticated { get; }

    /// <summary>
    /// 获取用户的首选语言/区域设置
    /// </summary>
    /// <remarks>
    /// 返回用户设置的语言代码（如：zh-CN、en-US），如果未设置则返回 null
    /// </remarks>
    string PreferredCulture { get; }

    /// <summary>
    /// 获取用户的时区
    /// </summary>
    /// <remarks>
    /// 返回用户设置的时区ID（如：China Standard Time），如果未设置则返回 null
    /// </remarks>
    string TimeZone { get; }

    /// <summary>
    /// 获取用户的头像URL
    /// </summary>
    string AvatarUrl { get; }

    /// <summary>
    /// 获取用户的最后登录时间
    /// </summary>
    DateTime? LastLoginTime { get; }

    /// <summary>
    /// 获取用户的最后登录IP
    /// </summary>
    string LastLoginIp { get; }

    /// <summary>
    /// 获取用户的创建时间
    /// </summary>
    DateTime CreationTime { get; }

    /// <summary>
    /// 获取用户的所有声明
    /// </summary>
    /// <remarks>
    /// 返回用户关联的所有声明（Claims）列表
    /// </remarks>
    IReadOnlyList<System.Security.Claims.Claim> Claims { get; }

    /// <summary>
    /// 判断当前用户是否属于指定的角色
    /// </summary>
    /// <param name="role">要检查的角色名称</param>
    /// <returns>如果用户属于指定角色返回 true，否则返回 false</returns>
    /// <remarks>
    /// 角色名称比较不区分大小写
    /// </remarks>
    /// <exception cref="ArgumentException">当角色名称为 null 或空字符串时抛出</exception>
    bool IsInRole(string role);

    /// <summary>
    /// 判断当前用户是否拥有指定的权限
    /// </summary>
    /// <param name="permission">要检查的权限名称</param>
    /// <returns>如果用户拥有指定权限返回 true，否则返回 false</returns>
    /// <remarks>
    /// 权限名称比较不区分大小写
    /// </remarks>
    /// <exception cref="ArgumentException">当权限名称为 null 或空字符串时抛出</exception>
    bool HasPermission(string permission);

    /// <summary>
    /// 判断当前用户是否拥有指定的所有权限
    /// </summary>
    /// <param name="permissions">要检查的权限名称列表</param>
    /// <returns>如果用户拥有所有指定权限返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentException">当权限列表为 null 或包含空字符串时抛出</exception>
    bool HasAllPermissions(IEnumerable<string> permissions);

    /// <summary>
    /// 判断当前用户是否拥有指定权限中的任意一个
    /// </summary>
    /// <param name="permissions">要检查的权限名称列表</param>
    /// <returns>如果用户拥有任意一个指定权限返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentException">当权限列表为 null 或包含空字符串时抛出</exception>
    bool HasAnyPermission(IEnumerable<string> permissions);

    /// <summary>
    /// 获取指定类型的声明值
    /// </summary>
    /// <param name="claimType">声明类型</param>
    /// <returns>声明值，如果不存在则返回 null</returns>
    string FindClaimValue(string claimType);

    /// <summary>
    /// 获取指定类型的所有声明值
    /// </summary>
    /// <param name="claimType">声明类型</param>
    /// <returns>声明值列表</returns>
    IReadOnlyList<string> FindClaimValues(string claimType);
} 
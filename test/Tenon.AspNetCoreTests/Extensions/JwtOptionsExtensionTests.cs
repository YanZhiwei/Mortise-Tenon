using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tenon.AspNetCore.Configuration;

namespace Tenon.AspNetCore.Extensions.Tests;

/// <summary>
/// JWT选项扩展方法的单元测试类
/// </summary>
[TestClass]
public class JwtOptionsExtensionTests
{
    private const string TestKey = "YourTestSecretKeyHere12345678901234567890123456789012345678901234567890";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";
    private const string TestRefreshAudience = "TestRefreshAudience";
    private JwtOptions _jwtOptions;

    /// <summary>
/// 测试初始化，设置JWT选项的基本配置
/// </summary>
    [TestInitialize]
    public void Setup()
    {
        _jwtOptions = new JwtOptions
        {
            SymmetricSecurityKey = TestKey,
            ValidIssuer = TestIssuer,
            ValidAudience = TestAudience,
            RefreshTokenAudience = TestRefreshAudience,
            Encoding = Encoding.UTF8,
            Expire = 30, // 30分钟
            RefreshTokenExpire = 1440, // 24小时
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = 5 // 5秒钟的时钟偏差
        };
    }

    /// <summary>
    /// 测试生成令牌验证参数
    /// 验证生成的参数与配置的选项一致
    /// </summary>
    [TestMethod]
    public void GenerateTokenValidationParameters_ValidOptions_ReturnsCorrectParameters()
    {
        // Act - 生成验证参数
        var parameters = _jwtOptions.GenerateTokenValidationParameters();

        // Assert - 验证参数值
        Assert.IsNotNull(parameters);
        Assert.AreEqual(_jwtOptions.ValidateIssuer, parameters.ValidateIssuer);
        Assert.AreEqual(_jwtOptions.ValidIssuer, parameters.ValidIssuer);
        Assert.AreEqual(_jwtOptions.ValidateIssuerSigningKey, parameters.ValidateIssuerSigningKey);
        Assert.AreEqual(_jwtOptions.ValidateAudience, parameters.ValidateAudience);
        Assert.AreEqual(_jwtOptions.ValidAudience, parameters.ValidAudience);
        Assert.AreEqual(_jwtOptions.ValidateLifetime, parameters.ValidateLifetime);
        Assert.AreEqual(_jwtOptions.RequireExpirationTime, parameters.RequireExpirationTime);
        Assert.AreEqual(TimeSpan.FromSeconds(_jwtOptions.ClockSkew), parameters.ClockSkew);
    }

    /// <summary>
    /// 测试使用空选项生成验证参数时抛出异常
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GenerateTokenValidationParameters_NullOptions_ThrowsArgumentNullException()
    {
        // Act - 使用null选项生成参数
        JwtOptions nullOptions = null;
        nullOptions.GenerateTokenValidationParameters();
    }

    /// <summary>
    /// 测试创建包含用户信息的访问令牌
    /// 验证令牌中包含完整的用户信息，包括用户ID、名称、邮箱、角色和租户ID
    /// </summary>
    [TestMethod]
    public void CreateAccessToken_WithUserInfo_ReturnsValidToken()
    {
        // Arrange - 准备用户声明数据
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim("tenant_id", "tenant1")
        };

        // Act - 创建访问令牌
        var result = _jwtOptions.CreateAccessToken(userClaims);

        // Assert - 验证令牌内容
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);

        Assert.AreEqual(TestIssuer, jwtToken.Issuer);
        Assert.AreEqual(TestAudience, jwtToken.Audiences.First());
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "123"));
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Name && c.Value == "testuser"));
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com"));
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "admin"));
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == "tenant_id" && c.Value == "tenant1"));
    }

    /// <summary>
    /// 测试创建包含用户信息的刷新令牌
    /// 验证刷新令牌中包含必要的用户信息（用户ID和租户ID）
    /// </summary>
    [TestMethod]
    public void CreateRefreshToken_WithUserInfo_ReturnsValidToken()
    {
        // Arrange - 准备基本用户声明数据
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("tenant_id", "tenant1")
        };

        // Act - 创建刷新令牌
        var result = _jwtOptions.CreateRefreshToken(userClaims);

        // Assert - 验证令牌内容
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);

        Assert.AreEqual(TestIssuer, jwtToken.Issuer);
        Assert.AreEqual(TestRefreshAudience, jwtToken.Audiences.First());
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "123"));
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == "tenant_id" && c.Value == "tenant1"));
    }

    /// <summary>
    /// 测试多租户场景下的令牌声明获取
    /// 验证不同租户的令牌能正确返回各自的声明信息
    /// </summary>
    [TestMethod]
    public void GetClaimsFromToken_WithMultiTenant_ReturnsTenantSpecificClaims()
    {
        // Arrange - 准备两个不同租户的声明数据
        var tenant1Claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("tenant_id", "tenant1"),
            new Claim(ClaimTypes.Role, "admin")
        };

        var tenant2Claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "456"),
            new Claim("tenant_id", "tenant2"),
            new Claim(ClaimTypes.Role, "user")
        };

        var tenant1Token = _jwtOptions.CreateAccessToken(tenant1Claims).Token;
        var tenant2Token = _jwtOptions.CreateAccessToken(tenant2Claims).Token;

        // Act - 获取两个租户的声明
        var tenant1ResultClaims = _jwtOptions.GetClaimsFromToken(tenant1Token);
        var tenant2ResultClaims = _jwtOptions.GetClaimsFromToken(tenant2Token);

        // Assert - 验证两个租户的声明信息
        Assert.IsNotNull(tenant1ResultClaims);
        Assert.IsNotNull(tenant2ResultClaims);

        // 验证租户1的声明
        Assert.IsTrue(tenant1ResultClaims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "123"));
        Assert.IsTrue(tenant1ResultClaims.Any(c => c.Type == "tenant_id" && c.Value == "tenant1"));
        Assert.IsTrue(tenant1ResultClaims.Any(c => c.Type == ClaimTypes.Role && c.Value == "admin"));

        // 验证租户2的声明
        Assert.IsTrue(tenant2ResultClaims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "456"));
        Assert.IsTrue(tenant2ResultClaims.Any(c => c.Type == "tenant_id" && c.Value == "tenant2"));
        Assert.IsTrue(tenant2ResultClaims.Any(c => c.Type == ClaimTypes.Role && c.Value == "user"));
    }

    /// <summary>
    /// 测试使用不同的受众验证令牌
    /// 验证当使用不匹配的受众时，令牌验证失败
    /// </summary>
    [TestMethod]
    public void GetClaimsFromToken_WithDifferentAudience_ReturnsClaims()
    {
        // Arrange - 准备令牌数据
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("tenant_id", "tenant1")
        };
        var token = _jwtOptions.CreateAccessToken(claims).Token;
        var differentAudience = "different_audience";

        // Act - 使用不同的受众验证令牌
        var resultClaims = _jwtOptions.GetClaimsFromToken(token, true, differentAudience);

        // Assert - 验证结果为空
        Assert.IsNull(resultClaims); // 因为受众不匹配，应返回null
    }

    /// <summary>
    /// 测试从有效令牌中获取声明
    /// </summary>
    [TestMethod]
    public void GetClaimsFromToken_ValidToken_ReturnsClaims()
    {
        // Arrange - 准备测试数据
        var originalClaims = new[] { new Claim("test", "value") };
        var token = _jwtOptions.CreateAccessToken(originalClaims).Token;

        // Act - 从令牌中获取声明
        var resultClaims = _jwtOptions.GetClaimsFromToken(token);

        // Assert - 验证声明内容
        Assert.IsNotNull(resultClaims);
        Assert.IsTrue(resultClaims.Any(c => c.Type == "test" && c.Value == "value"));
    }

    /// <summary>
    /// 测试过期令牌返回空结果
    /// </summary>
    [TestMethod]
    public async Task GetClaimsFromToken_ExpiredToken_ReturnsNull()
    {
        // Arrange - 准备过期的令牌
        _jwtOptions.Expire = 1; // 设置为1分钟后过期
        var claims = new[] { new Claim("test", "value") };
        var token = _jwtOptions.CreateAccessToken(claims).Token;
        
        // 等待令牌过期
        await Task.Delay(TimeSpan.FromMinutes(5)); // Wait longer than the token's lifetime

        // Act - 验证过期令牌
        var resultClaims = _jwtOptions.GetClaimsFromToken(token);

        // Assert - 验证结果为空
        Assert.IsNull(resultClaims);
    }

    /// <summary>
    /// 测试无效令牌返回空结果
    /// </summary>
    [TestMethod]
    public void GetClaimsFromToken_InvalidToken_ReturnsNull()
    {
        // Act - 验证无效令牌
        var resultClaims = _jwtOptions.GetClaimsFromToken("invalid.token.string");

        // Assert - 验证结果为空
        Assert.IsNull(resultClaims);
    }

    /// <summary>
    /// 测试空令牌抛出异常
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetClaimsFromToken_NullToken_ThrowsArgumentNullException()
    {
        // Act - 验证null令牌
        _jwtOptions.GetClaimsFromToken(null);
    }

    /// <summary>
    /// 测试使用刷新令牌获取新的访问令牌
    /// 验证：
    /// 1. 刷新令牌的有效性验证
    /// 2. 新访问令牌包含原始用户信息
    /// 3. 新访问令牌的有效期正确设置
    /// </summary>
    [TestMethod]
    public void RefreshToken_ValidRefreshToken_ReturnsNewAccessToken()
    {
        // Arrange - 准备用户信息和声明
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim("tenant_id", "tenant1")
        };

        // 创建原始访问令牌和刷新令牌
        var originalAccessToken = _jwtOptions.CreateAccessToken(userClaims);
        var refreshToken = _jwtOptions.CreateRefreshToken(userClaims);

        // 验证刷新令牌
        var refreshTokenClaims = _jwtOptions.GetClaimsFromToken(
            refreshToken.Token, 
            true, 
            _jwtOptions.RefreshTokenAudience);

        // Act - 使用刷新令牌获取新的访问令牌
        var newAccessToken = refreshTokenClaims != null 
            ? _jwtOptions.CreateAccessToken(refreshTokenClaims.ToArray(), _jwtOptions.ValidAudience)
            : null;

        // Assert - 验证新的访问令牌
        Assert.IsNotNull(newAccessToken);
        Assert.IsNotNull(newAccessToken.Token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(newAccessToken.Token);

        // 验证基本属性
        Assert.AreEqual(TestIssuer, jwtToken.Issuer);
        Assert.AreEqual(_jwtOptions.ValidAudience, jwtToken.Audiences.First());

        // 验证用户信息是否保持一致
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "123"));
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Name && c.Value == "testuser"));
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com"));
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "admin"));
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == "tenant_id" && c.Value == "tenant1"));

        // 验证新令牌的过期时间
        var expectedExpiration = DateTime.UtcNow.AddMinutes(_jwtOptions.Expire);
        Assert.IsTrue(Math.Abs((expectedExpiration - jwtToken.ValidTo).TotalSeconds) < 5); // 允许5秒误差
    }

    /// <summary>
    /// 测试使用过期的刷新令牌获取新的访问令牌
    /// 验证过期的刷新令牌无法用于获取新的访问令牌
    /// </summary>
    [TestMethod]
    public async Task RefreshToken_ExpiredRefreshToken_ReturnsNull()
    {
        // Arrange - 准备用户信息和声明
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("tenant_id", "tenant1")
        };

        // 设置非常短的刷新令牌过期时间
        _jwtOptions.RefreshTokenExpire = 0.0001m; // 约0.144分钟（8.64秒）

        var refreshToken = _jwtOptions.CreateRefreshToken(userClaims);

        Console.WriteLine($"RefreshTokenExpire: {_jwtOptions.RefreshTokenExpire}");
        Console.WriteLine($"RefreshTokenExpire in minutes: {(double)_jwtOptions.RefreshTokenExpire * 24 * 60}");

        // 额外调试信息
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(refreshToken.Token);
        Console.WriteLine($"Token ValidFrom: {jwtToken.ValidFrom}");
        Console.WriteLine($"Token ValidTo: {jwtToken.ValidTo}");
        Console.WriteLine($"Current UTC Time: {DateTime.UtcNow}");

        // 等待令牌过期
        await Task.Delay(TimeSpan.FromSeconds(10)); // 等待足够长的时间确保令牌过期

        // Act - 尝试使用过期的刷新令牌获取声明
        var refreshTokenClaims = _jwtOptions.GetClaimsFromToken(
            refreshToken.Token, 
            true, 
            _jwtOptions.RefreshTokenAudience);

        // Assert - 验证结果为空
        Assert.IsNull(refreshTokenClaims, "Expired refresh token should return null claims");
    }

    /// <summary>
    /// 测试使用错误受众的刷新令牌
    /// 验证使用错误受众的刷新令牌无法获取新的访问令牌
    /// </summary>
    [TestMethod]
    public void RefreshToken_WrongAudience_ReturnsNull()
    {
        // Arrange - 准备用户信息和声明
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("tenant_id", "tenant1")
        };

        var refreshToken = _jwtOptions.CreateRefreshToken(userClaims);

        // Act - 使用错误的受众验证刷新令牌
        var refreshTokenClaims = _jwtOptions.GetClaimsFromToken(
            refreshToken.Token, 
            true, 
            "wrong_audience");

        // Assert - 验证结果为空
        Assert.IsNull(refreshTokenClaims);
    }
}
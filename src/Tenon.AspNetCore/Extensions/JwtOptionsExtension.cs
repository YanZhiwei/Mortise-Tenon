using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Tenon.AspNetCore.Configuration;
using Tenon.AspNetCore.Models;

namespace Tenon.AspNetCore.Extensions;

public static class JwtOptionsExtension
{
    public static TokenValidationParameters GenerateTokenValidationParameters(this JwtOptions jwtOptions)
    {
        if (jwtOptions == null) throw new ArgumentNullException(nameof(jwtOptions));
        return new TokenValidationParameters
        {
            ValidateIssuer = jwtOptions.ValidateIssuer,
            ValidIssuer = jwtOptions.ValidIssuer,
            ValidateIssuerSigningKey = jwtOptions.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(jwtOptions.Encoding.GetBytes(jwtOptions.SymmetricSecurityKey)),
            ValidateAudience = jwtOptions.ValidateAudience,
            ValidAudience = jwtOptions.ValidAudience,
            ValidateLifetime = jwtOptions.ValidateLifetime,
            RequireExpirationTime = jwtOptions.RequireExpirationTime,
            ClockSkew = TimeSpan.FromSeconds(jwtOptions.ClockSkew)
        };
    }

    public static JwtBearerToken CreateAccessToken(this JwtOptions jwtOptions, Claim[] claims, string? audience = null)
    {
        // 如果没有提供受众，则使用默认有效受众
        audience ??= jwtOptions.ValidAudience;

        // 替换或添加受众声明
        var modifiedClaims = claims.ToList();
        var existingAudienceClaim = modifiedClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Aud);
        if (existingAudienceClaim != null)
        {
            modifiedClaims.Remove(existingAudienceClaim);
        }
        modifiedClaims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));

        // 生成令牌
        return WriteToken(jwtOptions, modifiedClaims.ToArray());
    }

    public static JwtBearerToken CreateRefreshToken(this JwtOptions jwtOptions, Claim[] claims)
    {
        if (jwtOptions == null) throw new ArgumentNullException(nameof(jwtOptions));
        if (!(claims?.Any() ?? false)) throw new ArgumentNullException(nameof(claims));
        return WriteToken(jwtOptions, claims, false);
    }

    public static JwtBearerToken CreateIdToken(this JwtOptions jwtOptions, Claim[] claims)
    {
        if (jwtOptions == null) throw new ArgumentNullException(nameof(jwtOptions));
        if (!(claims?.Any() ?? false)) throw new ArgumentNullException(nameof(claims));

        return WriteToken(jwtOptions, claims, false);
    }

    private static JwtBearerToken WriteToken(JwtOptions jwtOptions, Claim[] claims, bool accessToken = true)
    {
        var key = new SymmetricSecurityKey(jwtOptions.Encoding.GetBytes(jwtOptions.SymmetricSecurityKey));
        var issuer = jwtOptions.ValidIssuer;
        var audience = accessToken ? jwtOptions.ValidAudience : jwtOptions.RefreshTokenAudience;
        var now = DateTime.UtcNow.AddSeconds(-1); // Add a small buffer to account for slight time differences
        var expiresMinutes = accessToken ? jwtOptions.Expire : (double)jwtOptions.RefreshTokenExpire * 24 * 60;
        var expires = now.AddMinutes(expiresMinutes);

        // 确保必要的claims存在
        var claimsList = claims.ToList();

        // 添加nbf (Not Before)
        if (claimsList.All(c => c.Type != "nbf"))
            claimsList.Add(new Claim("nbf", new DateTimeOffset(now).ToUnixTimeSeconds().ToString()));

        // 添加exp (Expiration Time)
        if (claimsList.All(c => c.Type != "exp"))
            claimsList.Add(new Claim("exp", new DateTimeOffset(expires).ToUnixTimeSeconds().ToString()));

        // 添加iss (Issuer)
        if (claimsList.All(c => c.Type != "iss")) 
            claimsList.Add(new Claim("iss", issuer));

        // 添加aud (Audience)
        var existingAudClaim = claimsList.FirstOrDefault(c => c.Type == "aud");
        if (existingAudClaim == null)
        {
            claimsList.Add(new Claim("aud", audience));
        }
        else
        {
            // 移除现有的aud claim，并添加正确的claim
            claimsList.Remove(existingAudClaim);
            claimsList.Add(new Claim("aud", 
                accessToken ? jwtOptions.ValidAudience : jwtOptions.RefreshTokenAudience));
        }

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claimsList,
            now,
            expires,
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtBearerToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public static Claim[]? GetClaimsFromToken(this JwtOptions jwtOptions, string token, bool validateLifetime = true,
        string? audience = null)
    {
        if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

        var parameters = GenerateTokenValidationParameters(jwtOptions);
        parameters.ValidateLifetime = validateLifetime;
        if (audience != null) parameters.ValidAudience = audience;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var result = tokenHandler.ValidateToken(token, parameters, out var securityToken);

            if (result.Identity is null || !result.Identity.IsAuthenticated ||
                securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                return null;

            // 额外验证过期时间
            if (validateLifetime && securityToken.ValidTo <= DateTime.UtcNow)
                return null;

            return result.Claims.ToArray();
        }
        catch (SecurityTokenExpiredException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }
}
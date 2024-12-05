using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Tenon.AspNetCore.Identity.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long? GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return long.TryParse(userId, out var parsedUserId)
            ? parsedUserId
            : null;
    }
}
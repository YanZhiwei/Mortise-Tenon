using Microsoft.AspNetCore.Identity;

namespace Tenon.AspNetCore.Identity.Extensions
{
    public static class IdentityResultExtension
    {
        public static string GetErrorMessage(this IdentityResult? result)
        {
            if (result == null)
                return string.Empty;
            if (!result.Succeeded && (result.Errors?.Any() ?? false))
                return result.Errors.First().Description;
            return string.Empty;
        }
    }
}

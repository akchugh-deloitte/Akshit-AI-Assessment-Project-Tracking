using System.Security.Claims;

namespace ServiceApi.API.Utilities;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Get current authenticated user's ID from JWT NameIdentifier claim. Returns 0 if missing/invalid.</summary>
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var value) ? value : 0;
    }
}

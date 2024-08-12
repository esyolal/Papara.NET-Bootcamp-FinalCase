using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Pp.Business.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUser()
    {
        var name = httpContextAccessor.HttpContext?.User?.Identity?.Name;
        return name ?? "Unknown";
    }

    public long GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

        Console.WriteLine($"UserIdClaim: {userIdClaim}");

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new InvalidOperationException("User ID claim is not present.");
        }

        if (long.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        else
        {
            throw new InvalidOperationException("User ID is not valid.");
        }
    }

}

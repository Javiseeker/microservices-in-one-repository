using Microsoft.AspNetCore.Http;

using System.Security.Claims;

namespace AMR.Shared.Common.Auth;

public class AuthHelper : IAuthHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        var claimsIdentity = _httpContextAccessor.HttpContext!.User.Identity as ClaimsIdentity;
        string userId = claimsIdentity!.Claims.FirstOrDefault(x => x.Type == "preferred_username")!.Value;
        return userId[..userId.IndexOf('@')];
    }
}



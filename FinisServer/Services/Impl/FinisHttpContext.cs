using FinisServer.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FinisServer.Services.Impl;

public class FinisHttpContext(IHttpContextAccessor httpContextAccessor) : IFinisHttpContext
{
    public int? GetRequestUserId()
    {
        var sub = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub);
        if (sub == null)
        {
            return null;
        }
        return int.Parse(sub.Value);
    }
}
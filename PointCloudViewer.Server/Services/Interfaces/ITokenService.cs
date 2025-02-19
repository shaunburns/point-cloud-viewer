using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace PointCloudViewer.Server.Services.Interfaces
{
    public interface ITokenService
    {
        string? GenerateJwtToken(IdentityUser user);
        string? GetPrincipalIdentityNameFromExpiredToken(string token);
    }
}

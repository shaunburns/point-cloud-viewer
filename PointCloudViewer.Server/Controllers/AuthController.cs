using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PointCloudViewer.Server.DTOs;
using PointCloudViewer.Server.Services.Interfaces;

namespace PointCloudViewer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ITokenService tokenService) : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly ITokenService _tokenService = tokenService;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterNewUserRequestDTO dto)
        {
            var user = new IdentityUser { UserName = dto.Username, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                return Ok(new { Message = "User created successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDTO dto)
        {
            var result = await _signInManager.PasswordSignInAsync(dto.Username, dto.Password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(dto.Username);
                if (user != null)
                {
                    var token = _tokenService.GenerateJwtToken(user);
                    return Ok(new { Token = token });
                }
            }

            return Unauthorized();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> Refresh()
        {
            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var principalIdentityName = _tokenService.GetPrincipalIdentityNameFromExpiredToken(token);
            if (string.IsNullOrEmpty(principalIdentityName))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByNameAsync(principalIdentityName);
            if (user == null)
            {
                return Unauthorized();
            }

            var newToken = _tokenService.GenerateJwtToken(user);
            return Ok(new { Token = newToken });
        }
    }
}

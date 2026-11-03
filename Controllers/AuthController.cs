using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicketTitan.DTO;
using TicketTitan.Models;

namespace TicketTitan.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
           _config = config;
           _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new [] { _config["Authentication:Google:ClientId"]! }
                });

                var user = await _userManager.FindByLoginAsync("Google", payload.Subject);

                if(user is null)
                {
                    return Unauthorized("This Google account is not linked to any user.");
                }
                var roles = await _userManager.GetRolesAsync(user);

                var appToken = GenerateApplicationJwt(payload.Email, user.Id, roles);
                return Ok(new { Token = appToken, Email = payload.Email });
            }

            catch
            {
                return BadRequest("Invalid Google Token.");
            }
        }

        private string GenerateApplicationJwt(string email, string userId, IEnumerable<string> roles)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicketTitan.DTO;
using TicketTitan.Interfaces;

namespace TicketTitan.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger
            )
        {
           _authService = authService;
           _logger = logger;
        }

        [Route("api/[controller]/google-login")]
        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
        {
            try
            {
                var result = await _authService.ValidateGoogleLogin(request.IdToken);

                if (!result.IsSuccess || result.Value is null)
                {
                    return BadRequest(result.ErrorMessage);
                }

                var (user, roles, payload) = result.Value;

                var appToken = GenerateApplicationJwt(payload.Email, user.Id, roles);
                return Ok(new { Token = appToken, Email = payload.Email });
            }
            catch(InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Google ID-token validation failed.");
                return Unauthorized("Invalid Google ID-token.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while validating Google login.");
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Login could not be completed"
                    );
            }
        }

        private string GenerateApplicationJwt(string email, string userId, IEnumerable<string> roles)
        {
            var jwtSettings = _authService.GetJwtSettings();
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

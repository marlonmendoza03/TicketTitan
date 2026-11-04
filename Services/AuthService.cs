using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using TicketTitan.Common;
using TicketTitan.Data;
using TicketTitan.DTO;
using TicketTitan.Interfaces;
using TicketTitan.Models;

namespace TicketTitan.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _config = config;
            _roleManager = roleManager;
        }

        public async Task<ServiceResult<GoogleLoginResultDto>> ValidateGoogleLogin(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Authentication:Google:ClientId"]! }
            });

            if (string.IsNullOrWhiteSpace(payload.Subject) ||
                string.IsNullOrWhiteSpace(payload.Email) ||
                !payload.EmailVerified)
            {
                return ServiceResult<GoogleLoginResultDto>.Failure(
                    "A Google account with a verified email is required.");
            }

            var user = await _userManager.FindByLoginAsync("Google", payload.Subject);

            if (user is null)
            {
                var registration = await RegisterGoogleAccountAsync(
                    payload.Subject,
                    payload.Email);

                if (!registration.IsSuccess || registration.Value is null)
                {
                    return ServiceResult<GoogleLoginResultDto>.Failure(
                        registration.ErrorMessage ?? "Google account registration failed.");
                }

                user = registration.Value;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return ServiceResult<GoogleLoginResultDto>.Success(new GoogleLoginResultDto(user, roles, payload));
        }

        public IConfigurationSection GetJwtSettings()
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            return jwtSettings;
        }

        private async Task<ServiceResult<ApplicationUser>> RegisterGoogleAccountAsync(
            string googleSubject,
            string email)
        {

            if (await _userManager.FindByEmailAsync(email) is not null)
            {
                return ServiceResult<ApplicationUser>.Failure(
                    "An account already uses this email. Sign in to that account and link Google separately.");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                return ServiceResult<ApplicationUser>.Failure(
                    "Could not create the application account.");
            }

            var loginResult = await _userManager.AddLoginAsync(
                user,
                new UserLoginInfo("Google", googleSubject, "Google"));

            if (!loginResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                return ServiceResult<ApplicationUser>.Failure(
                    "Could not link the Google account.");
            }

            const string defaultRole = "User";

            if (!await _roleManager.RoleExistsAsync(defaultRole))
            {
                var createRoleResult =
                    await _roleManager.CreateAsync(new IdentityRole(defaultRole));

                if (!createRoleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return ServiceResult<ApplicationUser>.Failure(
                        "Could not create the default role.");
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, defaultRole);

            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return ServiceResult<ApplicationUser>.Failure(
                    "Could not assign the default role.");
            }

            return ServiceResult<ApplicationUser>.Success(user);
        }
    }
}

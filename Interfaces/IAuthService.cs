using TicketTitan.Common;
using TicketTitan.DTO;

namespace TicketTitan.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<GoogleLoginResultDto>> ValidateGoogleLogin(string idToken);
        IConfigurationSection GetJwtSettings();
    }
}

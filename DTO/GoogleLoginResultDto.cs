using Google.Apis.Auth;
using TicketTitan.Models;

namespace TicketTitan.DTO
{
    public record GoogleLoginResultDto
    (
        ApplicationUser User,
        IEnumerable<string> Roles,
        GoogleJsonWebSignature.Payload Payload
    );
}

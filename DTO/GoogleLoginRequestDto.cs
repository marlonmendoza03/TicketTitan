using System.ComponentModel.DataAnnotations;

namespace TicketTitan.DTO
{
    public sealed class GoogleLoginRequestDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}

using TicketTitan.DTO;

namespace TicketTitan.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketResponseDto>> GetAllTicketsAsync();
        Task<TicketResponseDto> GetTicketByIdAsync(int id);
        Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto);
        Task<bool> UpdateStatusAsync(int id, string newStatus);
    }
}

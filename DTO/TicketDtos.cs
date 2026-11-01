namespace TicketTitan.DTO
{
    public record CreateTicketDto(
        string Title, 
        string Description, 
        string Category, 
        int Priority
    );

    public record UpdateTicketStatusDto(string Status);

    public record TicketResponseDto(
        int Id,
        string Title,
        string Description,
        string Category,
        string Status,
        int Priority,
        DateTime CreatedAt
    );

    public record TicketNotificationDto(
        int TicketId,
        string Title,
        string Message,
        string Type,
        DateTime TimeStamp
    );
}

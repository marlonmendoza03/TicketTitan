using FluentValidation;
using TicketTitan.DTO;

namespace TicketTitan.Validators
{
    public class UpdateTicketStatusDtoValidator : AbstractValidator<UpdateTicketStatusDto>
    {
        private static readonly string[] AllowedStatuses = ["Open", "In Progress", "Resolved", "Closed"];

        public UpdateTicketStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => AllowedStatuses.Contains(status.ToString())).WithMessage($"Invalid Status. Allowed values are: {string.Join(",",AllowedStatuses)}.");
        }
    }
}

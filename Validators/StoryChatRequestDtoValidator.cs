using FluentValidation;
using TicketTitan.DTO;

namespace TicketTitan.Validators
{
    public class StoryChatRequestDtoValidator : AbstractValidator<StoryChatRequestDto>
    {
        public const int MaxMessages = 40;
        public const int MaxMessageLength = 4000;

        public StoryChatRequestDtoValidator()
        {
            RuleFor(x => x.Setup).NotNull().WithMessage("Story setup is required.");

            When(x => x.Setup is not null, () =>
            {
                RuleFor(x => x.Setup.CategoryName)
                    .NotEmpty().WithMessage("Category is required.")
                    .MaximumLength(100);

                RuleFor(x => x.Setup.EpisodeCount)
                    .InclusiveBetween(1, 20).WithMessage("Episode count must be between 1 and 20.");

                RuleFor(x => x.Setup.EpisodeDurationSec)
                    .InclusiveBetween(30, 600).WithMessage("Episode duration must be between 30 and 600 seconds.");
            });

            RuleFor(x => x.Messages)
                .NotEmpty().WithMessage("At least one message is required.")
                .Must(m => m.Count <= MaxMessages).WithMessage($"A chat can have at most {MaxMessages} messages.")
                .Must(m => m.Count > 0 && m[^1].Role == "user").WithMessage("The last message must be from the user.");

            RuleForEach(x => x.Messages).ChildRules(message =>
            {
                message.RuleFor(m => m.Role)
                    .Must(role => role is "user" or "assistant").WithMessage("Role must be 'user' or 'assistant'.");

                message.RuleFor(m => m.Content)
                    .NotEmpty().WithMessage("Message content is required.")
                    .MaximumLength(MaxMessageLength);
            });
        }
    }
}

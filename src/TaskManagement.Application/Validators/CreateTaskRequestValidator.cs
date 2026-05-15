using FluentValidation;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(3);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.DueDate)
            .Must(date => date == null || date > DateTime.UtcNow)
            .WithMessage("DueDate must be in the future");

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
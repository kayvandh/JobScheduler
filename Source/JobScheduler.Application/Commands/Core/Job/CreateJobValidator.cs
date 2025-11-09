using FluentValidation;
using Framework.Schedule.Extensions;

namespace JobScheduler.Application.Commands.Core.Job
{
    public class CreateJobValidator : AbstractValidator<CreateJobCommand>
    {
        public CreateJobValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage(Resource.ValidationMessages.NotEmpty)
                .MaximumLength(200).WithMessage(Resource.ValidationMessages.MaximumLength);

            RuleFor(p => p.CronSchedule)
                .NotEmpty().WithMessage(Resource.ValidationMessages.NotEmpty)
                .Must(c => c.IsValidCron()).WithMessage(Resource.ValidationMessages.InvalidFormat);

            RuleFor(p => p.Status)
                .NotEmpty().WithMessage(Resource.ValidationMessages.NotEmpty);
        }
    }
}
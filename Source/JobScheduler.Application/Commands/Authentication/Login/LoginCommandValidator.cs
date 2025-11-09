using FluentValidation;

namespace JobScheduler.Application.Commands.Authentication.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(Resource.ValidationMessages.NotEmpty)
                .MaximumLength(20).WithMessage(Resource.ValidationMessages.MaximumLength);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Resource.ValidationMessages.NotEmpty)
                .MinimumLength(8).WithMessage(Resource.ValidationMessages.MinimumLength);
        }
    }
}
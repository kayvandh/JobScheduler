using FluentResults;
using Framework.FluentResultsAddOn;
using MediatR;
using JobScheduler.Application.Common.Interfaces.Identity;

namespace JobScheduler.Application.Commands.Authentication.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly Common.Interfaces.Identity.IAuthenticationService _authenticationService;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(Common.Interfaces.Identity.IAuthenticationService authenticationService, ITokenService tokenService)
        {
            _authenticationService = authenticationService;
            _tokenService = tokenService;
        }

        public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var userId = await _authenticationService.ValidateUserAsync(request.Username, request.Password);
            if (userId is null)
                return Result.Fail(FluentError.Raise(ApplicationErrorCode.NotFound, "Invalid Username or Password"));

            var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(userId.Value);

            return Result.Ok(new LoginResponse(accessToken, refreshToken));
        }
    }
}
using FluentResults;
using Framework.FluentResultsAddOn;
using MediatR;
using JobScheduler.Application.Common.Interfaces.Identity;

namespace JobScheduler.Application.Commands.Authentication.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
    {
        private readonly ITokenService _tokenService;
        private readonly Common.Interfaces.Identity.IAuthenticationService _authenticationService;

        public RefreshTokenCommandHandler(ITokenService tokenService, IAuthenticationService authenticationService)
        {
            _tokenService = tokenService;
            _authenticationService = authenticationService;
        }

        public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refresh = await _tokenService.GetRefreshTokenAsync(request.RefreshToken);
            if (refresh is null || !refresh.IsActive)
                return Result.Fail(FluentError.Raise(ApplicationErrorCode.BadRequest, "Invalid or expired refresh token."));

            var validOrActive = await _authenticationService.IsUserValidOrActiveAsync(refresh.UserId);
            if (!validOrActive) return Result.Fail(FluentError.Raise(ApplicationErrorCode.NotFound, "User not found."));

            var newToken = await _tokenService.RotateRefreshTokenAsync(refresh.UserId, request.RefreshToken);
            var (access, refreshToken) = await _tokenService.GenerateTokensAsync(refresh.UserId);

            return Result.Ok(new RefreshTokenResponse(access, newToken.Token));
        }
    }
}
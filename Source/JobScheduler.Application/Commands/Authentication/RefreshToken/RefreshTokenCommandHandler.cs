using FluentResults;
using Framework.FluentResultsAddOn;
using MediatR;
using JobScheduler.Application.Common.Interfaces.Identity;
using Framework.Common;

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
                return Result.Fail(FluentError.Raise(ApplicationErrorCode.BadRequest, Resource.Messages.InvalidValue.FormatWith("Refresh Token")));

            var validOrActive = await _authenticationService.IsUserValidOrActiveAsync(refresh.UserId);
            if (!validOrActive) return Result.Fail(FluentError.Raise(ApplicationErrorCode.NotFound, Resource.Messages.NotFound.FormatWith("User")));

            var newToken = await _tokenService.RotateRefreshTokenAsync(refresh.UserId, request.RefreshToken);
            var (access, refreshToken) = await _tokenService.GenerateTokensAsync(refresh.UserId);

            return Result.Ok(new RefreshTokenResponse(access, newToken.Token));
        }
    }
}
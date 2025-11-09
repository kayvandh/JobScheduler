using FluentResults;
using MediatR;
using JobScheduler.Application.Common.Interfaces.Identity;

namespace JobScheduler.Application.Commands.Authentication.RevokeToken
{
    public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
    {
        private readonly ITokenService _tokenService;

        public RevokeTokenCommandHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
            return Result.Ok();
        }
    }
}
using FluentResults;
using MediatR;

namespace JobScheduler.Application.Commands.Authentication.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;

    public record RefreshTokenResponse(string AccessToken, string RefreshToken);
}
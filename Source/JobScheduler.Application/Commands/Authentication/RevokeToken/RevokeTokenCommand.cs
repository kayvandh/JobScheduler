using FluentResults;
using MediatR;

namespace JobScheduler.Application.Commands.Authentication.RevokeToken
{
    public record RevokeTokenCommand(string RefreshToken) : IRequest<Result>;
}
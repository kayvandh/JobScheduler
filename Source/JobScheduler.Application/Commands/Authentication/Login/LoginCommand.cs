using FluentResults;
using MediatR;

namespace JobScheduler.Application.Commands.Authentication.Login
{
    public record LoginCommand(string Username, string Password) : IRequest<Result<LoginResponse>>;

    public record LoginResponse(string AccessToken, string RefreshToken);
}
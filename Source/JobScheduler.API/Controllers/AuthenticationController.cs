using Framework.ApiResponse;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using JobScheduler.API.Common;
using JobScheduler.API.Common.Attributes;
using JobScheduler.Application.Commands.Authentication.Login;
using JobScheduler.Application.Commands.Authentication.RefreshToken;
using JobScheduler.Application.Commands.Authentication.RevokeToken;

namespace JobScheduler.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : BaseApiController
    {
        private readonly IMediator _mediator;

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ApiResponse))]
        [AllowAnonymousPermission]
        public async Task<ApiResponse<LoginResponse>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            return FromResult<LoginResponse>(await _mediator.Send(command, cancellationToken));
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ApiResponse))]
        public async Task<ApiResponse<RefreshTokenResponse>> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            return FromResult<RefreshTokenResponse>(await _mediator.Send(command, cancellationToken));
        }

        [HttpPost("revoke")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ApiResponse))]
        public async Task<ApiResponse> Revoke([FromBody] RevokeTokenCommand command, CancellationToken cancellationToken)
        {
            return FromResult(await _mediator.Send(command, cancellationToken));
        }
    }
}
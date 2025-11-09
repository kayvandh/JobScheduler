using Framework.ApiResponse;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using JobScheduler.API.Common;
using JobScheduler.Application.Commands.Core.Job;

namespace JobScheduler.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : BaseApiController
    {
        private readonly IMediator _mediator;

        public JobController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesDefaultResponseType(typeof(ApiResponse))]
        public async Task<ApiResponse<Guid>> Login([FromBody] CreateJobCommand command)
        {
            return FromResult<Guid>(await _mediator.Send(command));
        }
    }
}
using FluentResults;
using Framework.ApiResponse;
using Microsoft.AspNetCore.Mvc;

namespace JobScheduler.API.Common
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected ApiResponse<T> FromResult<T>(Result<T> result)
        {
            return result.ToApiResponse<T>();
        }

        protected ApiResponse FromResult(Result result)
        {
            return result.ToApiResponse();
        }
    }
}
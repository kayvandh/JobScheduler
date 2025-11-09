using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json.Serialization;

namespace Framework.ApiResponse
{
    public class ApiResponse : IActionResult
    {
        public bool HasError { get; set; }
        public string? Message { get; set; }
        public List<Error>? Errors { get; set; }

        [JsonIgnore]
        public HttpStatusCode HttpStatusCode { get; set; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode;
            await new ObjectResult(this).ExecuteResultAsync(context);
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }
    }
}
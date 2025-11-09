using FluentValidation;
using Framework.ApiResponse;
using System.Net;
using System.Text.Json;

namespace JobScheduler.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error");

                var errors = ex.Errors.Select(e => new Framework.ApiResponse.Error(e.PropertyName, e.ErrorMessage)).ToList();

                var response = new ApiResponse
                {
                    HasError = true,
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Message = "Validation failed",
                    Errors = errors
                };

                await WriteResponseAsync(context, response, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                var response = new ApiResponse
                {
                    HasError = true,
                    HttpStatusCode = HttpStatusCode.InternalServerError,
                    Message = "Unexpected error occurred",
                    Errors = new List<Error>
                    {
                        new Error("INTERNAL_ERROR", _env.IsDevelopment() ? ex.Message : "Something went wrong")
                    }
                };

                await WriteResponseAsync(context, response, HttpStatusCode.InternalServerError);
            }
        }

        private async Task WriteResponseAsync(HttpContext context, ApiResponse response, HttpStatusCode statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
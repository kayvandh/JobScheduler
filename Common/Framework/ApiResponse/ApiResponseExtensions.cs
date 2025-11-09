using FluentResults;
using Framework.FluentResultsAddOn;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Framework.ApiResponse
{
    public static class ApiResponseExtensions
    {
        public static ApiResponse<T> ToApiResponse<T>(this FluentResults.Result<T> result)
        {
            var response = new ApiResponse<T>
            {
                HasError = !result.IsSuccess,
                Message = result.IsSuccess
                    ? result.Successes.FirstOrDefault()?.Message ?? string.Empty
                    : result.Errors.FirstOrDefault()?.Message ?? string.Empty,
                Errors = result.IsSuccess
                    ? result.Successes.Select(s => new Error(s.Message, "Success")).ToList()
                    : result.Errors.Select(e => new Error(e.Message, e.Metadata != null && e.Metadata.TryGetValue("Code", out object? value) && value != null ? value.ToString()! : "General")).ToList(),
                HttpStatusCode = result.IsSuccess ? HttpStatusCode.OK : GetStatusCodeFromErrors(result.Errors.ToList()),
                Data = result.IsSuccess ? result.Value : default
            };

            return response;
        }

        public static ApiResponse ToApiResponse(this FluentResults.Result result)
        {
            var response = new ApiResponse()
            {
                HasError = !result.IsSuccess,
                Message = result.IsSuccess
                    ? result.Successes.FirstOrDefault()?.Message ?? string.Empty
                    : result.Errors.FirstOrDefault()?.Message ?? string.Empty,
                Errors = result.IsSuccess
                    ? result.Successes.Select(s => new Error(s.Message, "Success")).ToList()
                    : result.Errors.Select(e => new Error(e.Message, e.Metadata != null && e.Metadata.TryGetValue("Code", out object? value) && value != null ? value.ToString()! : "General")).ToList(),
                HttpStatusCode = result.IsSuccess ? HttpStatusCode.OK : GetStatusCodeFromErrors(result.Errors.ToList()),
            };

            return response;
        }

        public static IActionResult ToApiResponse(this ActionContext context)
        {
            var errors = context.ModelState
                .Where(m => m.Value?.Errors.Count > 0)
                .SelectMany(p =>
                    p.Value.Errors.Select(e => new Error(e.ErrorMessage, p.Key)))
                .ToList();

            return new ApiResponse
            {
                HasError = true,
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = "One or more validation errors occurred.",
                Errors = errors
            };
        }

        private static HttpStatusCode GetStatusCodeFromErrors(List<FluentResults.IError> errors)
        {
            foreach (var error in errors)
            {
                if (error is FluentError fluentError)
                {
                    return fluentError.ErrorCode switch
                    {
                        ApplicationErrorCode.Unauthorized => HttpStatusCode.Unauthorized,
                        ApplicationErrorCode.Forbidden => HttpStatusCode.Forbidden,
                        ApplicationErrorCode.NotFound => HttpStatusCode.NotFound,
                        ApplicationErrorCode.BadRequest => HttpStatusCode.BadRequest,
                        _ => HttpStatusCode.InternalServerError
                    };
                }
                else
                {
                    if (error.Metadata != null && error.Metadata.TryGetValue("StatusCode", out var code))
                    {
                        if (code is int intCode && Enum.IsDefined(typeof(HttpStatusCode), intCode))
                            return (HttpStatusCode)intCode;
                        if (code is string strCode && int.TryParse(strCode, out int parsed) && Enum.IsDefined(typeof(HttpStatusCode), parsed))
                            return (HttpStatusCode)parsed;
                    }
                }
            }

            return HttpStatusCode.InternalServerError;
        }
    }
}
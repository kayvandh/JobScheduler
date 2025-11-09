using Microsoft.AspNetCore.Mvc.Controllers;
using JobScheduler.API.Common.Attributes;
using JobScheduler.Application.Common.Interfaces.Identity;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace JobScheduler.API.Middlewares
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HashSet<string> _exactWhitelist;
        private readonly List<Regex> _patternWhitelist;
        private readonly HashSet<string> _ipWhitelist;

        public AuthorizationMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;

            _exactWhitelist = config.GetSection("Authorization:ExactWhitelist").Get<HashSet<string>>()
                              ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            _patternWhitelist = config.GetSection("Authorization:PatternWhitelist").Get<List<string>>()?
                .Select(p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToList()
                ?? new List<Regex>();

            _ipWhitelist = config.GetSection("Authorization:IPWhiteList").Get<HashSet<string>>() ??
                            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task InvokeAsync(HttpContext context, IAuthorizationService authorizationService)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            var httpMethod = context.Request.Method.ToUpperInvariant();

            var remoteIp = context.Connection.RemoteIpAddress;
            if (remoteIp == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                await context.Response.WriteAsync("Unacceptable ip address");
                return;
            }

            if (IsIPWhiteListed(remoteIp))
            {
                await _next(context);
                return;
            }

            if (IsWhitelisted(path))
            {
                await _next(context);
                return;
            }

            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (descriptor != null)
                {
                    bool hasAllowAnonymousAttr =
                        descriptor.MethodInfo.GetCustomAttribute<AllowAnonymousPermissionAttribute>() != null
                        || descriptor.ControllerTypeInfo.GetCustomAttribute<AllowAnonymousPermissionAttribute>() != null;

                    if (hasAllowAnonymousAttr)
                    {
                        await _next(context);
                        return;
                    }
                }
            }

            if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var isAuthorized = await authorizationService.AuthorizeAsync(context.User, path, httpMethod);

            if (!isAuthorized)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Forbidden: You do not have permission to access this resource.");
                return;
            }
            await _next(context);
        }

        private bool IsWhitelisted(string path)
        {
            if (_exactWhitelist.Contains(path))
                return true;

            foreach (var regex in _patternWhitelist)
            {
                if (regex.IsMatch(path))
                    return true;
            }

            return false;
        }

        private bool IsIPWhiteListed(IPAddress remoteIp)
        {
            string ipString = remoteIp.ToString();
            if (_ipWhitelist.Contains(ipString))
                return true;

            return false;
        }
    }
}
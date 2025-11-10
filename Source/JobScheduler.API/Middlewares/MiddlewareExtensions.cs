namespace JobScheduler.API.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApiPermissionAuthorization(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthorizationMiddleware>();
        }

        public static IApplicationBuilder UseGeneralExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }

        public static IApplicationBuilder UseLogWithCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
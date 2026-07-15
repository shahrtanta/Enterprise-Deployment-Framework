using Product.Reference.Web.Services;

namespace Product.Reference.Web.Middleware;

public sealed class FirstRunMiddleware
{
    private readonly RequestDelegate _next;

    public FirstRunMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        RuntimeConfigurationService runtimeConfiguration)
    {
        if (runtimeConfiguration.IsSetupComplete()
            || IsAllowedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        context.Response.Redirect("/Setup");
    }

    private static bool IsAllowedPath(PathString path)
    {
        return path.StartsWithSegments("/Setup")
            || path.StartsWithSegments("/health")
            || path.StartsWithSegments("/css")
            || path.StartsWithSegments("/js")
            || path.StartsWithSegments("/images")
            || path.StartsWithSegments("/fonts")
            || path.StartsWithSegments("/favicon.ico");
    }
}

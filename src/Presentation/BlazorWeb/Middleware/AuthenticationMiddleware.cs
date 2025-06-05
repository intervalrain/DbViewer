using Application.Contracts.Services;

namespace Presentation.BlazorWeb.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var path = context.Request.Path.Value?.ToLower();
        
        // 允許訪問登入頁面和靜態資源
        if (path == "/login" || 
            path?.StartsWith("/_framework") == true ||
            path?.StartsWith("/_content") == true ||
            path?.StartsWith("/css") == true ||
            path?.StartsWith("/js") == true ||
            path?.StartsWith("/favicon") == true ||
            path?.StartsWith("/_blazor") == true)
        {
            await _next(context);
            return;
        }

        // 檢查是否已認證
        if (!authService.IsAuthenticated && path != "/login")
        {
            // 對於 Blazor SignalR 連接，返回 401 而不是重定向
            if (path?.StartsWith("/_blazor") == true)
            {
                context.Response.StatusCode = 401;
                return;
            }
            
            context.Response.Redirect("/login");
            return;
        }

        await _next(context);
    }
} 
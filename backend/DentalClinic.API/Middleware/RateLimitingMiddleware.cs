using System.Net;

namespace DentalClinic.API.Middleware;

public class RateLimitingMiddleware(
    RequestDelegate next,
    ILogger<RateLimitingMiddleware> logger)
{
    private static readonly Dictionary<string, List<DateTime>> _attempts = new();
    private static readonly object _lock = new();
    
    // Configuration: 5 attempts per 15 minutes
    private const int MaxAttempts = 5;
    private readonly TimeSpan _window = TimeSpan.FromMinutes(15);

    public async Task InvokeAsync(HttpContext context)
    {
        // Only rate limit POST /api/auth/login
        if (context.Request.Path.StartsWithSegments("/api/auth/login") && 
            context.Request.Method == "POST")
        {
            var clientIp = GetClientIp(context);
            
            lock (_lock)
            {
                CleanUpOldAttempts();
                
                if (!_attempts.ContainsKey(clientIp))
                {
                    _attempts[clientIp] = new List<DateTime>();
                }
                
                var attempts = _attempts[clientIp];
                
                if (attempts.Count >= MaxAttempts)
                {
                    logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);
                    
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.Headers.Append("Retry-After", _window.TotalSeconds.ToString("0"));
                    
                    return;
                }
                
                attempts.Add(DateTime.UtcNow);
            }
        }
        
        await next(context);
    }

    private static string GetClientIp(HttpContext context)
    {
        // Check for forwarded headers (proxy/load balancer)
        if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? 
                   context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
        
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private void CleanUpOldAttempts()
    {
        var cutoff = DateTime.UtcNow.Subtract(_window);
        
        foreach (var key in _attempts.Keys.ToList())
        {
            _attempts[key] = _attempts[key]
                .Where(attempt => attempt > cutoff)
                .ToList();
            
            if (_attempts[key].Count == 0)
            {
                _attempts.Remove(key);
            }
        }
    }
}

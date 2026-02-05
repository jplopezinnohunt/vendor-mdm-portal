using System.Security.Claims;

namespace VendorMdm.Api.Middleware;

public class MockAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MockAuthMiddleware> _logger;

    public MockAuthMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<MockAuthMiddleware> logger)
    {
        _next = next;
        _env = env;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only allow in Development and if user is not already authenticated
        if (_env.IsDevelopment() && !context.User.Identity.IsAuthenticated)
        {
            // Check header first, then query string (for SignalR WebSocket connections)
            var mockUserHeader = context.Request.Headers["X-Mock-User"].FirstOrDefault();

            // For SignalR negotiate/connect, check query string since WebSockets can't send custom headers
            if (string.IsNullOrEmpty(mockUserHeader) && context.Request.Path.StartsWithSegments("/hubs"))
            {
                mockUserHeader = context.Request.Query["mockUser"].FirstOrDefault();
            }
            
            if (!string.IsNullOrEmpty(mockUserHeader))
            {
                _logger.LogInformation("MockAuthMiddleware: Authenticating as {Role}", mockUserHeader);

                try 
                {
                    // Create claims based on the mock role
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, $"Mock {mockUserHeader}"),
                        new Claim(ClaimTypes.NameIdentifier, $"mock-{mockUserHeader.ToLower()}-001"),
                        new Claim(ClaimTypes.Email, $"mock.{mockUserHeader.ToLower()}@example.com"), // Add Email claim
                        // Add specific roles
                        new Claim(ClaimTypes.Role, mockUserHeader)
                    };

                    // Also add 'Approver' role if the role is Admin or BFM or VendorUnit or Requestor
                    if (mockUserHeader == "Admin" || mockUserHeader == "BFM" || mockUserHeader == "VendorUnit" || mockUserHeader == "Requestor")
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Approver"));
                    }

                    var identity = new ClaimsIdentity(claims, "MockAuth"); // "MockAuth" is the authentication type
                    var principal = new ClaimsPrincipal(identity);

                    context.User = principal;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set mock user");
                }
            }
        }

        await _next(context);
    }
}

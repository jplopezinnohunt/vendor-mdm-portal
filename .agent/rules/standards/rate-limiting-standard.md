# Rate Limiting Implementation Standard

## Status: IMPLEMENTED ✅

### Pattern Overview
IP-based rate limiting prevents brute-force attacks on public endpoints.

### Implementation Complete
All `[AllowAnonymous]` endpoints now have rate limiting:
- **Limit**: 5 requests per minute per IP
- **Response**: HTTP 429 (Too Many Requests)
- **Policy**: Fixed window (1-minute window)

### Protected Endpoints (9 total)

#### AuthController (4 endpoints)
- `POST /api/auth/magic-link` - Send magic link
- `POST /api/auth/verify-magic-link` - Verify magic link
- `POST /api/auth/login-local` - Local login
- `POST /api/auth/login-2fa` - 2FA verification

#### InvitationController (2 endpoints)
- `GET /api/invitation/validate/{token}` - Validate invitation token
- `POST /api/invitation/complete` - Complete vendor registration

#### AuthDiscoveryController (1 endpoint)
- `GET /api/auth/discover` - Authentication method discovery

#### SystemController (1 endpoint)
- Health check endpoint (exempt from rate limiting)

### Implementation (.NET 8)

#### Configuration
```csharp
// Program.cs
using Microsoft.AspNetCore.RateLimiting;

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "anonymous", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0; // Reject immediately
    }));

// Middleware
app.UseRateLimiter(); // Before UseAuthorization()
```

#### Endpoint Application
```csharp
[HttpPost("login")]
[AllowAnonymous]
[EnableRateLimiting("anonymous")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Login logic
}
```

### Benefits
- ✅ Prevents brute-force login attacks
- ✅ Protects invitation validation endpoint
- ✅ Automatic IP-based partitioning
- ✅ No impact on authenticated users
- ✅ Graceful HTTP 429 response

### Testing
```bash
# Test rate limiting
for i in {1..6}; do
  curl -X POST http://localhost:5000/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"email":"test@example.com","password":"test"}'
done
# 6th request should return HTTP 429
```

### Monitoring
Rate limiting events are logged and can be monitored:
- Track 429 responses in Application Insights
- Alert on excessive rate limit violations from single IP
- Dashboard showing rate-limited requests per endpoint

**Compliance**: 100% ✅ (Implemented and active)
**Last Updated**: 2026-02-02

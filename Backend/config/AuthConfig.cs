using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using beikeon.api.middleware;
using beikeon.domain.security;
using beikeon.domain.Security;
using beikeon.domain.user;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace beikeon.config;

public static class AuthConfig {
    public static void ConfigureAuthorization(this IServiceCollection serviceCollection, IConfiguration configuration) {
        serviceCollection.AddAuthorization(o => {
            o.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }

    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration) {
        var jwtConfig = configuration.GetSection("Authentication").GetSection("JWT");
        var jwtKeyStr = jwtConfig["Key"] ?? throw new ValidationException("JWT Key not found in config!");
        var jwtKey = Encoding.UTF8.GetBytes(jwtKeyStr);
        var expiryMins = jwtConfig["ExpiryMins"] ?? throw new ValidationException("Expiry minutes not found in config!");

        TokenGenerator.Initialize(jwtKeyStr, int.Parse(expiryMins));

        services.AddAuthentication().AddJwtBearer(o => {
            o.Events = new JwtBearerEvents {
                OnTokenValidated = HandleOnTokenValidated,
                OnAuthenticationFailed = HandleOnAuthenticationFailed
            };

            o.TokenValidationParameters = GetTokenValidationParameters(jwtKey);
        });
    }

    private static Task HandleOnTokenValidated(TokenValidatedContext context) {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

        var userEmail = context.Principal?.Identity?.Name;
        var userIdStr = context.Principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userEmail is null || userIdStr is null) {
            context.Fail($"Missing user information in token [{userIdStr} {userEmail}]");
            return Task.CompletedTask;
        }

        var userId = long.Parse(userIdStr);

        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        var securityContext = context.HttpContext.RequestServices.GetRequiredService<ISecurityContext>();

        securityContext.Initialize(() => userService.MustGetUserById(long.Parse(userIdStr)), userId, userEmail);

        logger.LogInformation("User {UserName} [{UserId}] authenticated successfully via JWT", userEmail, userIdStr);
        return Task.CompletedTask;
    }

    private static Task HandleOnAuthenticationFailed(AuthenticationFailedContext context) {
        context.Response.Cookies.Delete(AuthMiddleware.AuthCookieName);
        return Task.CompletedTask;
    }

    private static TokenValidationParameters GetTokenValidationParameters(byte[] jwtKey) {
        return new TokenValidationParameters {
            ValidateIssuer = false,
            // ValidIssuer = jwtConfig["ValidIssuer"],
            ValidateAudience = false,
            // ValidAudience = jwtConfig["ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateIssuerSigningKey = true
            // ValidateLifetime = true,
            // ClockSkew = TimeSpan.Zero,
            // LifetimeValidator = (before, expires, token, parameters) => {
            //     var tokenLifetimeMinutes = (expires - before)?.TotalMinutes;
            //     return tokenLifetimeMinutes <= 10; // Maximum Token Lifespan
            // }
        };
    }
}
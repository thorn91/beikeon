using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using beikeon.config;
using beikeon.data;
using beikeon.domain.security;
using beikeon.domain.user;
using beikeon.web;
using beikeon.web.middleware;
using beikeon.web.security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISecurityContext, SecurityContext>();
builder.Services.AddDbContext<BeikeonDbContext>(
    options => { options.UseNpgsql(DatabaseConfig.GetConnString(builder)); });

builder.Services.AddAuthorization(o => {
    o.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddAuthentication().AddJwtBearer(o => {
    var jwtConfig = builder.Configuration.GetSection("Authentication").GetSection("JWT");
    var jwtKeyStr = jwtConfig["Key"] ?? throw new ValidationException("JWT Key not found in config!");
    var jwtKey = Encoding.UTF8.GetBytes(jwtKeyStr);
    var expiryMins = jwtConfig["ExpiryMins"] ?? throw new ValidationException("Expiry minutes not found in config!");

    TokenGenerator.Initialize(jwtKeyStr, int.Parse(expiryMins));

    o.Events = new JwtBearerEvents {
        OnTokenValidated = context => {
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
        },
        OnAuthenticationFailed = context => {
            context.Response.Cookies.Delete(AuthMiddleware.AuthCookieName);
            return Task.CompletedTask;
        }
    };

    o.TokenValidationParameters = new TokenValidationParameters {
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
});

builder.Services.AddHttpLogging(logging => {
    logging.LoggingFields = HttpLoggingFields.All;
    logging.MediaTypeOptions.AddText("application/javascript");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

/* Add services to the container.
 * Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle */
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

var app = builder.Build();
app.UseMiddleware<AuthMiddleware>();

// Configure the HTTP request pipeline and create tables that do not exist
if (app.Environment.IsDevelopment()) {
    app.Logger.LogInformation("Environment => Development");

    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BeikeonDbContext>();
    var dbResult = context.Database.EnsureCreated();
    Console.WriteLine($"Database created: {dbResult}");
}

app.UseHttpLogging();

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.MapGet("/", () => "Hello, World!");

app.MapGet("/users", async (BeikeonDbContext context) => {
    var users = await context.Users.ToListAsync();
    return users;
});

app.MapPost("/users", async (BeikeonDbContext context, User user) => {
    await context.Users.AddAsync(user);
    await context.SaveChangesAsync();
    return user;
});

app.MapGroup(AuthEndpointsExt.Prefix)
    .MapAuthEndpoints()
    .AllowAnonymous();

app.Run();
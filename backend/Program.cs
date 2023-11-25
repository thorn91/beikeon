using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using beikeon.config;
using beikeon.data;
using beikeon.domain.security;
using beikeon.domain.user;
using beikeon.web;
using beikeon.web.security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer(o => {
    var jwtConfig = builder.Configuration.GetSection("Authentication").GetSection("JWT");
    var jwtKey = jwtConfig["Key"] ?? throw new ValidationException("JWT Key not found in config!");
    var expiryMins = jwtConfig["ExpiryMins"] ?? throw new ValidationException("Expiry minutes not found in config!");

    TokenGenerator.Initialize(jwtKey, int.Parse(expiryMins));

    o.Events = new JwtBearerEvents {
        OnTokenValidated = async context => {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

            var userName = context.Principal?.Identity?.Name;
            var userId = context.Principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userName is null || userId is null) context.Fail($"Missing user information in token [{userId} {userName}]");

            logger.LogInformation("User {UserName} [{UserId}] authenticated successfully via JWT", userName, userId);
        }
    };

    o.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = false,
        ValidIssuer = jwtConfig["ValidIssuer"],
        ValidateAudience = false,
        ValidAudience = jwtConfig["ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
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

builder.Services.AddDbContext<BeikeonDbContext>(
    options => { options.UseNpgsql(DatabaseConfig.GetConnString(builder)); });

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISecurityContext, SecurityContext>();

// options.UseInMemoryDatabase("User")
// .ConfigureWarnings(wcb => wcb.Ignore(InMemoryEventId.TransactionIgnoredWarning));

var app = builder.Build();

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
}).RequireAuthorization();

app.MapGroup("/login")
    .MapAuthEndpoints()
    .AllowAnonymous();

app.Run();
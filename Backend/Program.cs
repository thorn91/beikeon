using beikeon.config;
using beikeon.data;
using beikeon.domain.security;
using beikeon.domain.user;
using beikeon.web;
using beikeon.web.middleware;
using beikeon.web.security;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISecurityContext, SecurityContext>();

builder.Services.ConfigureDbContext(configuration);

builder.Services.ConfigureAuthorization(configuration);
builder.Services.ConfigureAuthentication(configuration);

builder.Services.AddHttpLogging(logging => {
    logging.LoggingFields = HttpLoggingFields.All;
    logging.MediaTypeOptions.AddText("application/javascript");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle 
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
    var isDbCreated = context.Database.EnsureCreated();

    if (isDbCreated) {
        app.Logger.LogInformation("DB Setup for development!");
    }
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
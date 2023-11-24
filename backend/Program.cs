using api.config;
using api.data;
using api.domain.user;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer("Bearer");

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
});

app.Run();
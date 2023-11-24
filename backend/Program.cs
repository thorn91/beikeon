using api.config;
using api.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

builder.Services
    .AddDbContext<BeikeonDbContext>(options => {
        options.UseNpgsql(DatabaseConfig.GetConnString(builder));

        // options.UseInMemoryDatabase("User")
        // .ConfigureWarnings(wcb => wcb.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    Console.WriteLine("Development environment detected");

    app.UseSwagger();
    app.UseSwaggerUI();

    // Ensure Created will create the database if it does not exist, not just check connections
    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BeikeonDbContext>();
    var dbResult = context.Database.EnsureCreated();

    Console.WriteLine($"Database created: {dbResult}");
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.Run();
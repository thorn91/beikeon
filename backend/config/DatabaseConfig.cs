using System.ComponentModel.DataAnnotations;

namespace api.config;

public static class DatabaseConfig {
    private const string FallbackPgsqlConnStringTemplate = "Host={0};Username={1};Database={2};Port={3};Password={4};";
    private const string UseLocalDbEnvVarKey = "USE_LOCAL_DB";

    /// <summary>
    /// Returns the database string to use for the application.
    ///
    /// If the USE_LOCAL_DB environment variable is set to true, the hardcoded Local connection string will be used,
    /// otherwise it will be constructed from the Template connection string and environmental variables set
    /// (probably from docker compose).  Last ditch, it will hail mary and try to use a default connection string.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns>database connection string</returns>
    /// <exception cref="ValidationException"></exception>
    public static string GetConnString(WebApplicationBuilder builder) {
        if (Environment.GetEnvironmentVariable(UseLocalDbEnvVarKey) == "true") {
            return builder.Configuration.GetConnectionString("Local") ??
                   throw new ValidationException("Local Database use indicated without Conn String set in appsettings.json");
        }
        
        var connStringTemplate =
            builder.Configuration.GetConnectionString("Template") ?? FallbackPgsqlConnStringTemplate;

        var dbConfig = builder.Configuration
            .GetSection("Database")
            .GetSection("EnvVarNames");

        var userEnvStr = dbConfig["User"] ?? "POSTGRES_USER";
        var hostEnvStr = dbConfig["Host"] ?? "POSTGRES_HOST";
        var passwordEnvStr = dbConfig["Password"] ?? "POSTGRES_PASSWORD";
        var portEnvStr = dbConfig["Port"] ?? "POSTGRES_PORT";
        var databaseEnvStr = dbConfig["Database"] ?? "POSTGRES_DB";

        var user = Environment.GetEnvironmentVariable(userEnvStr) ?? "postgres";
        var password = Environment.GetEnvironmentVariable(passwordEnvStr) ?? "postgres";
        var port = Environment.GetEnvironmentVariable(portEnvStr) ?? "5432";
        var database = Environment.GetEnvironmentVariable(databaseEnvStr) ?? "beikeon";
        var host = Environment.GetEnvironmentVariable(hostEnvStr) ?? "db";

        return string.Format(connStringTemplate, host, user, database, port, password);
    }
}
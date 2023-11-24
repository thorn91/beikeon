namespace api.config;

public static class DatabaseConfig {
    private const string FallbackPgsqlConnStringTemplate = "Host={0};Username={1};Database={2};Port={3};Password={4};";
    
    public static string GetConnString(WebApplicationBuilder builder) {
        var connStringTemplate = builder.Configuration.GetConnectionString("Template") ?? FallbackPgsqlConnStringTemplate;
        
        var dbConfig = builder.Configuration
            .GetSection("Database")
            .GetSection("EnvVarNames");

        var userEnvStr = dbConfig["User"] ?? "postgres";
        var hostEnvStr = dbConfig["Host"] ?? "localhost";
        var passwordEnvStr = dbConfig["Password"] ?? "postgres";
        var portEnvStr = dbConfig["Port"] ?? "5432";
        var databaseEnvStr = dbConfig["Database"] ?? "beikeon";

        var user = Environment.GetEnvironmentVariable(userEnvStr);
        var password = Environment.GetEnvironmentVariable(passwordEnvStr);
        var port = Environment.GetEnvironmentVariable(portEnvStr);
        var database = Environment.GetEnvironmentVariable(databaseEnvStr);
        var host = Environment.GetEnvironmentVariable(hostEnvStr);

        return string.Format(connStringTemplate, host, user, database, port, password);
    }
    
}
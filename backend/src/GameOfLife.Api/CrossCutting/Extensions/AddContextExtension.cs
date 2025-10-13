namespace GameOfLife.CrossCutting.Extensions;

public static class AddContextExtension
{
    public static string BuildPostgresConnectionString(this IConfigurationBuilder builder, IHostEnvironment environment)
    {
        builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        var dbHost = configuration["DB_HOST"] ?? configuration["Database:Host"];
        var dbPort = configuration["DB_PORT"] ?? configuration["Database:Port"];
        var dbName = configuration["DB_NAME"] ?? configuration["Database:Name"];
        var dbUser = configuration["DB_USER"] ?? configuration["Database:User"];
        var dbPassword = configuration["DB_PASSWORD"] ?? configuration["Database:Password"];

        if (string.IsNullOrWhiteSpace(dbHost) ||
            string.IsNullOrWhiteSpace(dbPort) ||
            string.IsNullOrWhiteSpace(dbName) ||
            string.IsNullOrWhiteSpace(dbUser) ||
            string.IsNullOrWhiteSpace(dbPassword))
                throw new ArgumentException("Error to set up connection string.");

        return $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
    }
}
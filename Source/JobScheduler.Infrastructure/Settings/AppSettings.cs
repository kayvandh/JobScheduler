namespace JobScheduler.Infrastructure.Settings
{
    public class AppSettings
    {
        public static string SectionName => "AppSettings";

        public ConnectionStrings ConnectionStrings { get; set; } = new();
        public Logging Logging { get; set; } = new();
        public CacheSettings CacheSettings { get; set; } = new();
        public AuthorizationSettings Authorization { get; set; } = new();
        public JwtSettings Jwt { get; set; } = new();
        public CorsSettings CorsSettings { get; set; } = new();
        public string AllowedHosts { get; set; } = "*";
        public SerilogSettings Serilog { get; set; } = new();
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; } = string.Empty;
        public string Redis { get; set; } = string.Empty;
    }

    public class Logging
    {
        public LogLevel LogLevel { get; set; } = new();
    }

    public class LogLevel
    {
        public string Default { get; set; } = "Information";
        public string MicrosoftAspNetCore { get; set; } = "Warning";
    }

    public class CacheSettings
    {
        public string Provider { get; set; } = "Memory";
        public int DefaultExpirationMinutes { get; set; } = 60;
    }

    public class AuthorizationSettings
    {
        public List<string> ExactWhitelist { get; set; } = new();
        public List<string> PatternWhitelist { get; set; } = new();
        public List<string> IPWhiteList { get; set; } = new();
    }

    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int? AccessTokenExpiryMinutes { get; set; } = 30;
        public int? RefreshTokenExpiryDays { get; set; } = 14;
    }

    public class CorsSettings
    {
        public List<string> AllowedOrigins { get; set; } = new();
        public List<string> AllowedMethods { get; set; } = new();
        public List<string> AllowedHeaders { get; set; } = new();
        public bool AllowCredentials { get; set; }
    }

    public class SerilogSettings
    {
        public List<string>? Using { get; set; }
        public MinimumLevelOptions? MinimumLevel { get; set; }
        public List<string>? Enrich { get; set; }
        public List<WriteToOptions>? WriteTo { get; set; }
    }

    public class MinimumLevelOptions
    {
        public string? Default { get; set; }
        public Dictionary<string, string>? Override { get; set; }
    }

    public class WriteToOptions
    {
        public string? Name { get; set; }
        public WriteToArgs? Args { get; set; }
    }

    public class WriteToArgs
    {
        public string? NodeUris { get; set; }
        public string? IndexFormat { get; set; }
        public bool? AutoRegisterTemplate { get; set; }
        public int? NumberOfShards { get; set; }
        public int? NumberOfReplicas { get; set; }
    }
}

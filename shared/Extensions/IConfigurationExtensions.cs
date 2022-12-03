namespace Microsoft.Extensions.Configuration;

public static class IConfigurationExtensions
{
    public static T GetIotConfiguration<T>(this IConfiguration configuration) where T : class, new()
    {
        var config = new T();
        configuration.GetSection("Iot").Bind(config);
        return config;
    }
}
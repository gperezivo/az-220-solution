namespace Microsoft.Extensions.Configuration;

public static class IConfigurationBuilderExntesions
{
    public static IConfigurationBuilder AddCustomConfiguration<T>(this IConfigurationBuilder builder)
        where T: class
    {
        return builder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<T>()
                .AddEnvironmentVariables();        
    }
}
namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCustomLogging(this IServiceCollection services)
    {
        return services.AddLogging(builder =>
        {
            builder.AddSerilog(
                new LoggerConfiguration()
                    .WriteTo.Console()
                    .MinimumLevel.Debug()
                    .CreateLogger()
            );
        });
    }

}
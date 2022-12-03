using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Az220.Shared
{
    public interface ISensor
    {
        Message CreateMessage();
        Action<DeviceClient, ILogger?> Send {get;set;}
        Message CreateLogMessage();
        
    }
}
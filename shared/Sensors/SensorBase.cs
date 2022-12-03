using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Az220.Shared
{
    public abstract class SensorBase : ISensor
    {

        public SensorBase()
        {
            _sendAction = DefaultSendAction;
        }
        public virtual Message CreateMessage() => new Message(Encoding.ASCII.GetBytes(MessageString));
        public abstract string MessageString {get;}
        
        protected virtual Action<DeviceClient,ILogger?> DefaultSendAction =>
            (DeviceClient device, ILogger? log) => {                
                device.SendEventAsync(CreateMessage()).ConfigureAwait(false);
                log?.LogDebug($"Sent message: {MessageString}");
            };
        private Action<DeviceClient, ILogger?> _sendAction;
        public virtual Action<DeviceClient, ILogger?> Send {get => _sendAction;set => _sendAction = value;}


        
    }
}
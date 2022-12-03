using Az220.Shared.Configuration;
using Az220.Shared.Sensors;

var config = new ConfigurationBuilder()
    .AddCustomConfiguration<Program>()
    .Build();

var serviceCollection = new ServiceCollection()
    .AddCustomLogging()
    .BuildServiceProvider();
var log = serviceCollection.GetRequiredService<ILogger<Program>>();

var deviceConfig = config.GetIotConfiguration<Az220DeviceConfiguration>();

var sensor = new EnvironmentSensor();


var deviceClient = DeviceClient.CreateFromConnectionString(deviceConfig.DeviceConnectionString, TransportType.Mqtt);
while(true) {
    sensor.Send(deviceClient, log);
    await Task.Delay(1000);
}

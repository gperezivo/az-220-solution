var config = new ConfigurationBuilder()
    .AddCustomConfiguration<Program>()
    .Build();

var serviceCollection = new ServiceCollection()
    .AddCustomLogging()
    .BuildServiceProvider();
var log = serviceCollection.GetRequiredService<ILogger<Program>>();


var sensorConfig = config.GetIotConfiguration<Az220DeviceConfiguration>();
var sensor = new ConveyorBeltSimulator(2000);

var deviceClient = DeviceClient.CreateFromConnectionString(sensorConfig.DeviceConnectionString, TransportType.Mqtt);
await deviceClient.OpenAsync().ConfigureAwait(false);



while(true){
    sensor.ReadVibration();
    sensor.Send(deviceClient, log);
    await Task.Delay(2000).ConfigureAwait(false);
}
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

var serviceCollection = new ServiceCollection()
    .AddLogging(builder=> builder.AddSerilog(
        new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            .CreateLogger()
    )).BuildServiceProvider();
var log = serviceCollection.GetRequiredService<ILogger<Program>>();

var devicecs = config.GetValue<string>("DeviceConnectionString");
var deviceid = config.GetValue<string>("DeviceId");
static string CreateMessageString(double temp, double hum) => JsonConvert.SerializeObject(new { temperature = temp, humidity = hum });
var sensor = new EnvironmentSensor();


Action<DeviceClient> send = async (DeviceClient device) => {
    var temp = sensor.Temperature;
    var hum = sensor.Humidity;
    var message = new Message(Encoding.ASCII.GetBytes(CreateMessageString(temp, hum)));
    message.Properties.Add("temperatureAlert", (temp > sensor.TemperatureThreshold) ? "true" : "false");
    await device.SendEventAsync(message);
    log.LogDebug($"Sent message: {temp}°C, {hum}%");
};
var deviceClient = DeviceClient.CreateFromConnectionString(devicecs, TransportType.Mqtt);
while(true) {
    send(deviceClient);
    await Task.Delay(1000);
}

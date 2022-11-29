#region "Config"
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();
#endregion
#region "Logging"
var serviceCollection = new ServiceCollection()
    .AddLogging(builder=> builder.AddSerilog(
        new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            .CreateLogger()
    )).BuildServiceProvider();
var log = serviceCollection.GetRequiredService<ILogger<Program>>();
#endregion
#region "Sensor"
var sensor = new Sensor();
Action<DeviceClient> send = async (DeviceClient device) => {
    var temp = sensor.Temperature;
    var hum = sensor.Humidity;
    var location = sensor.GetLocation;
    var pressure = sensor.Pressure;
    var message = new Message(Encoding.ASCII.GetBytes(CreateMessageString(temp,hum,location,pressure)));
    message.Properties.Add("temperatureAlert", (temp > sensor.TemperatureThreshold) ? "true" : "false");
    await device.SendEventAsync(message);
    log.LogDebug($"Sent message: {temp}°C, {hum}%, {location}, {pressure}hPa");
};
string CreateMessageString(double temp, double hum, Sensor.Location location, double pressure) => 
    JsonConvert.SerializeObject(new { temperature = temp, humidity = hum, location = location, pressure = pressure });
#endregion
#region "Load Device Provisioning Service (DPS) settings"
var registrationId = config.GetValue<string>("IndividualDPS:RegistrationId");
var scopeId = config.GetValue<string>("IndividualDPS:ScopeId");
var primaryKey = config.GetValue<string>("IndividualDPS:SymmetricKey:PrimaryKey");
var secondaryKey = config.GetValue<string>("IndividualDPS:SymmetricKey:SecondaryKey");
const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";
#endregion

var telemetryDelay = 1;

using var security = new SecurityProviderSymmetricKey(registrationId, primaryKey, secondaryKey);
using var transport = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly);

var provisioningClient = ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, scopeId, security, transport);
var result = await provisioningClient.RegisterAsync().ConfigureAwait(false);

log.LogInformation($"Provisioning AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");
if(result.Status != ProvisioningRegistrationStatusType.Assigned) {
    log.LogError($"Provisioning failed: {result.Status}");
    log.LogError($"Provisioning failed: {result.ErrorMessage}");
    return;
}
var auth = new DeviceAuthenticationWithRegistrySymmetricKey(result.DeviceId, security.GetPrimaryKey());
var deviceClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Amqp);
await deviceClient.OpenAsync().ConfigureAwait(false);
log.LogInformation("Device connected.");
async Task OnDesiredPropertyChanged(
            TwinCollection desiredProperties,
            object userContext)
        {
            log.LogInformation($"Desired property change:{desiredProperties.ToJson()}");
            if(desiredProperties.Contains("telemetryDelay"))
            {
                string desiredDelay = desiredProperties["telemetryDelay"];
                if(desiredDelay != null)
                {
                    telemetryDelay = int.Parse(desiredDelay);
                    log.LogInformation($"Telemetry delay set to {telemetryDelay}");
                }
                var reportedProperties = new TwinCollection();
                reportedProperties["telemetryDelay"] = telemetryDelay.ToString();
                await deviceClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
                log.LogInformation($"Reported twin properties: {reportedProperties.ToJson()}");

            }
        };
await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, null).ConfigureAwait(false);

var twin = await deviceClient.GetTwinAsync().ConfigureAwait(false);
log.LogInformation($"Initial twin properties: {twin.Properties}");
await OnDesiredPropertyChanged(twin.Properties.Desired, null);


while(true) {
    send(deviceClient);
    await Task.Delay(telemetryDelay * 1000);
}

await deviceClient.CloseAsync().ConfigureAwait(false);







using Az220.Shared.Sensors;
using Az220.Shared.Configuration;

#region "Config"
var config = new ConfigurationBuilder()
    .AddCustomConfiguration<Program>()
    .Build();
#endregion
#region "Logging"
var serviceCollection = new ServiceCollection()
    .AddCustomLogging()
    .BuildServiceProvider();
var log = serviceCollection.GetRequiredService<ILogger<Program>>();
#endregion

var sensor = new ContainerSensor();



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
    sensor.Send(deviceClient, log);
    await Task.Delay(telemetryDelay * 1000);
}

await deviceClient.CloseAsync().ConfigureAwait(false);







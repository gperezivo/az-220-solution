using System.Security.Cryptography.X509Certificates;
using Az220.Shared.Configuration;
using Az220.Shared.Sensors;
#region "Config"


var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();
#endregion
#region "Logging"
var serviceCollection = new ServiceCollection()
    .AddCustomLogging()
    .BuildServiceProvider();
var log = serviceCollection.GetRequiredService<ILogger<Program>>();
#endregion
#region "Sensor"
var sensor = new ContainerSensor();

#endregion
#region "Load Device Provisioning Service (DPS) settings"
var dpsConfig = config.GetIotConfiguration<Az220GroupDeviceProvisioningConfiguration>();
#endregion

var telemetryDelay = 1;

#region "Certificate handling"
var certificatePath = args?.FirstOrDefault(arg => arg.StartsWith("--certificate"))?.Split("=").LastOrDefault()?.Trim();
dpsConfig.CertificatePath = certificatePath ?? dpsConfig.CertificatePath;
// Load the certificate from the file system
var cert = new X509Certificate2(dpsConfig.CertificatePath, 
                                dpsConfig.CertificatePassword);

#endregion
using var security = new SecurityProviderX509Certificate(cert);
//using var security = new SecurityProviderSymmetricKey(registrationId, primaryKey, secondaryKey);
using var transport = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly);

var provisioningClient = ProvisioningDeviceClient.Create(Az220Configuration.GlobalDeviceEndpoint, dpsConfig.ScopeId, security, transport);
var result = await provisioningClient.RegisterAsync().ConfigureAwait(false);

log.LogInformation($"Provisioning AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");
if(result.Status != ProvisioningRegistrationStatusType.Assigned) {
    log.LogError($"Provisioning failed: {result.Status}");
    log.LogError($"Provisioning failed: {result.ErrorMessage}");
    return;
}
//var auth = new DeviceAuthenticationWithRegistrySymmetricKey(result.DeviceId, security.GetPrimaryKey());
var auth = new DeviceAuthenticationWithX509Certificate(result.DeviceId, security.GetAuthenticationCertificate());
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
                reportedProperties["module"] = "GroupDPS";
                
                if(desiredProperties.Contains("test-value"))
                {
                    log.LogInformation($"Test value set to {desiredProperties["test-value"]}");
                    reportedProperties["test-value"] = desiredProperties["test-value"];
                }

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







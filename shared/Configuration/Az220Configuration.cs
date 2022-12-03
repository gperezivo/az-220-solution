namespace Az220.Shared.Configuration;

public class Az220Configuration
{   
    public const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";
    public string PrimaryKey {get;set;} = string.Empty;
    public string SecondaryKey {get;set;} = string.Empty;
}

public class Az220DeviceConfiguration : Az220Configuration
{
    public string DeviceConnectionString {get;set;} = string.Empty;
    public string DeviceId {get;set;} = string.Empty;

}
public abstract class Az220DeviceProvisioningConfiguration : Az220Configuration
{
    public string ScopeId {get;set;} = string.Empty;
}
public class Az200IndividualDeviceProvisioningConfiguration : Az220DeviceProvisioningConfiguration
{
    
    public string RegistrationId {get;set;} = string.Empty;
}

public class Az220GroupDeviceProvisioningConfiguration : Az220DeviceProvisioningConfiguration
{
    public string CertificatePath {get;set;} = string.Empty;
    public string CertificatePassword {get;set;} = string.Empty;
}
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Az220.Shared.Sensors;

public class ContainerSensor : SensorBase, ISensor
{
    double minTemp = 20;
    double minHum = 60;

    double minLatitude = 39.810492;
    double minLongitude = -98.556061;

    double minPressure = 1013.25;

    Random rand = new Random();


    public double Temperature => minTemp + rand.NextDouble() * 15;
    public double Humidity => minHum + rand.NextDouble() * 20;
    public double TemperatureThreshold => 30;
    public Location Location => new Location(minLatitude + rand.NextDouble() * 0.5, minLongitude + rand.NextDouble() * 0.5);
    public double Pressure => minPressure + rand.NextDouble() * 12;


    public override string MessageString =>
        JsonConvert.SerializeObject(
                new { 
                        temperature = Temperature, 
                        humidity = Humidity, 
                        location = Location, 
                        pressure = Pressure 
                    });
        
}

public record Location (double Latitude, double Longitude);
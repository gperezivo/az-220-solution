namespace Az220.Shared.Sensors;
public class EnvironmentSensor : SensorBase, ISensor
{
    double minTemp = 20;
    double minHum = 60;

    Random rand = new Random();

    public double Temperature => minTemp + rand.NextDouble() * 15;
    public double Humidity => minHum + rand.NextDouble() * 20;
    public double TemperatureThreshold {get;set;} = 30;

    public override string MessageString => JsonConvert.SerializeObject(new { temperature = Temperature, humidity = Humidity });
    public override Message CreateMessage()
    {
        var msg =  base.CreateMessage();
        msg.Properties.Add("temperatureAlert", (Temperature > TemperatureThreshold) ? "true" : "false");
        return msg;
    }
}


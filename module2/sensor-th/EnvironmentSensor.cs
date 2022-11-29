internal class EnvironmentSensor 
{
    double minTemp = 20;
    double minHum = 60;

    Random rand = new Random();

    public double Temperature => minTemp + rand.NextDouble() * 15;
    public double Humidity => minHum + rand.NextDouble() * 20;
    public double TemperatureThreshold => 30;
}
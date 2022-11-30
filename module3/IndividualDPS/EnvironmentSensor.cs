internal class Sensor 
{
    double minTemp = 20;
    double minHum = 60;

    double minLatitude = 39.810492;
    double minLongitude = -98.556061;

    double minPressure = 1013.25;

    Random rand = new Random();
    internal record Location(double Latitude, double Longitude);


    public double Temperature => minTemp + rand.NextDouble() * 15;
    public double Humidity => minHum + rand.NextDouble() * 20;
    public double TemperatureThreshold => 30;
    public Location GetLocation => new Location(minLatitude + rand.NextDouble() * 0.5, minLongitude + rand.NextDouble() * 0.5);
    public double Pressure => minPressure + rand.NextDouble() * 12;
}
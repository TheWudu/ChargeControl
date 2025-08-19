namespace ChargeControl;

public class PowerCalculations
{
    public static int KwToAmpere(double kW, double voltage = 400.0, double powerFactor = 1.0)
    {
        var amps = 1000 * kW / (Math.Sqrt(3) * powerFactor * voltage);
        var floorAmps = Math.Floor(1000 * kW / (Math.Sqrt(3) * powerFactor * voltage));
        var calcKw = Math.Sqrt(3) * powerFactor * floorAmps * voltage / 1000;

        var calcKw1P = powerFactor * floorAmps * voltage / 1000.0;
        
        Console.WriteLine($"kW: {kW:0.0} -> amps: {amps:00.00} -> {floorAmps} -> load with: {calcKw:0.00} kW or {calcKw1P:0.00} kW");

        return (int)floorAmps;
    }

}
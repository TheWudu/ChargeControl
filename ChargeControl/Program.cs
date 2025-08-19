// See https://aka.ms/new-console-template for more information

using ChargeControl;
using ChargeControl.Charger;
using ChargeControl.PowerPlant;

Console.WriteLine("Hello, World!");

int amps = 8;
double? maxPower = null;

if (args.Length > 0)
{
    if (args.Contains("-amps"))
    {
        var index = args.ToList().FindIndex(s => s == "-amps");
        amps = Convert.ToInt16(args.ElementAt(index + 1));
    }
    
    if (args.Contains("-max"))
    {
        var index = args.ToList().FindIndex(s => s == "-max");
        maxPower = Convert.ToDouble(args.ElementAt(index + 1));
    }
}

var charger = new GoeCharger();
_ = await charger.ReadValues();

if (charger.Values is not null)
{
    Console.WriteLine($"wh: {charger.Values.wh} Wh, dwo: {charger.Values.dwo}, amp: {charger.Values.amp}");
    Console.WriteLine($"cus: {charger.Values.cus}, frc: {charger.Values.frc}");
    Console.WriteLine($"eto: {charger.Values.eto / 1000.0} kWh, acu: {charger.Values.acu}");
    Console.WriteLine($"car: {charger.Values.car}, modelState: {charger.Values.modelStatus}");
    Console.WriteLine($"err: {charger.Values.err}");
}

// for (int i = 0; i < 100; i++)
// {
//     var fronius = new FroniusClient();
//     await fronius.Fetch();
//
//     Console.WriteLine("----");
//     Console.WriteLine(
//         $"Currently PV: {fronius.CurrentPowerFlowProducing() / 1000.0} kWh, Load: {fronius.CurrentPowerFlowLoad() / 1000.0} kWh, Grid: {fronius.CurrentPowerFlowGrid()} Wh, Akku: {fronius.CurrentPowerFlowAkku()} Wh");
//     Console.WriteLine($"Current SOC Level: {fronius.CurrentSocLevel()} from {fronius.SocCapacity()} Wh");
//
//     AdaptCharging(fronius, charger);
//
//     System.Threading.Thread.Sleep(1000);
// }

for (double i = 2.0; i < 8.5; i += 0.5)
{
    KwToAmpere(i);
}

var froniusClient = new FroniusApiClient();
var controller = new ChargeController(charger, froniusClient, amps, maxPower);

Console.CancelKeyPress += delegate(object? sender, ConsoleCancelEventArgs e) {
    e.Cancel = true;
    controller.Stop();
};

await controller.Setup();
await controller.Run();

return;

int KwToAmpere(double kW)
{
    var powerFactor = 1.0;
    var voltage = 400;
   
    var amps = 1000 * kW / (Math.Sqrt(3) * powerFactor * voltage);
    var floorAmps = Math.Floor(1000 * kW / (Math.Sqrt(3) * powerFactor * voltage));
    var calcKw = Math.Sqrt(3) * powerFactor * floorAmps * voltage / 1000;

    var calcKw1P = powerFactor * floorAmps * voltage / 1000.0;
    
    Console.WriteLine($"kW: {kW:0.0} -> amps: {amps:00.00} -> {floorAmps} -> load with: {calcKw:0.00} kW or {calcKw1P:0.00} kW");

    return (int)floorAmps;
}

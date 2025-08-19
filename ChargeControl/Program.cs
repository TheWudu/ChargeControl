// See https://aka.ms/new-console-template for more information

using ChargeControl;
using ChargeControl.Charger;
using ChargeControl.Charger.GoeChargerApi;
using ChargeControl.PowerPlant;
using ChargeControl.PowerPlant.FroniusApi;

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

var froniusClient = new FroniusApiClient();
var controller = new ChargeController(charger, froniusClient, amps, maxPower);

Console.CancelKeyPress += delegate(object? sender, ConsoleCancelEventArgs e) {
    e.Cancel = true;
    controller.Stop();
};

await controller.Setup();
await controller.Run();

return;


// See https://aka.ms/new-console-template for more information

using ChargeControl;
using ChargeControl.Charger;
using ChargeControl.Charger.GoeChargerApi;
using ChargeControl.PowerPlant;
using ChargeControl.PowerPlant.FroniusApi;

Console.WriteLine("Hello, World!");

int? amps = null;
double? maxPower = null;

var argsParser = new ArgumentsParser(args);
amps = argsParser.GetInt("minAmpere", true);
maxPower = argsParser.GetDouble("maxPower", true);

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


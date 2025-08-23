using ChargeControl;
using ChargeControl.Charger.GoeChargerApi;
using ChargeControl.PowerPlant.FroniusApi;

var argsParser = new ArgumentsParser(args);
var amps = argsParser.GetInt("minAmpere", true);
var maxPower = argsParser.GetDouble("maxPower", true);

var charger = new GoeCharger();
var froniusClient = new FroniusApiClient();
var controller = new ChargeController(charger, froniusClient, amps, maxPower);

Console.CancelKeyPress += delegate(object? _, ConsoleCancelEventArgs e) {
    e.Cancel = true;
    controller.Stop();
};

await controller.Setup();
await controller.Run();


Console.WriteLine("Exiting ...");
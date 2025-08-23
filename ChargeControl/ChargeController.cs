using ChargeControl.Charger;
using ChargeControl.Charger.GoeChargerApi;
using ChargeControl.PowerPlant;

namespace ChargeControl;

public class ChargeController(
    ICharger charger,
    IPowerPlant powerPlant,
    int? minimalAmps = null,
    double? maximumPowerToCharge = null)
{
    private States _state = States.NoCar;
    private bool _stopped;
    
    private const double MinimalPowerPvEnable = 4.0;
    private const double MinimalPowerPvDisable = 2.5;
    private const double MinimalSocLevelEnable = 50.0;
    private const double MinimalSocLevelDisable = 30.0;

    private readonly int _minimalAmpere = minimalAmps ?? 8;
    private const int MaximumAmps = 16;
    private const int UpdateInterval = 5000;

    public async Task Setup()
    {
        await UpdateValues();

        // start with minimal Amps
        if (charger.CurrentLimit() != _minimalAmpere)
        {
            await charger.SetCurrentLimit(_minimalAmpere);
        }

        // set maximum power to charge if given
        if (maximumPowerToCharge != null && !charger.PowerLimit().Equals(maximumPowerToCharge))
        {
            await charger.SetPowerLimit((double)maximumPowerToCharge);
        }
    }

    public async Task Run()
    {
        _stopped = false;
        
        Console.WriteLine($"Minimum Ampere: {_minimalAmpere}, Max Power: {maximumPowerToCharge}");
        
        while (!_stopped)
        {
            await UpdateValues();
            PrintValues();

            UpdateState();
            
            switch (_state)
            {
                case States.NoCar: HandleNoCar(); break;
                case States.Connected: HandleConnected(); break;
                case States.Charging: await HandleCharging(); break;
                case States.Stopped: HandleStopped(); break;
            }
            
            Thread.Sleep(UpdateInterval);
        }
        
        Console.WriteLine("Stopping controller ... ");
    }

    public void Stop()
    {
        _stopped = true;
    }

    private void PrintValues()
    {
        Console.WriteLine($"Charger: Loaded into car: {charger.ChargedPower():00.00} KWh, " +
                          $"PowerLimit: {charger.PowerLimit()} kWh, Current Amps: {charger.CurrentLimit()} A, " +
                          $"CarStatus: {charger.CarStatus()}");

        Console.WriteLine(
            $"PowerSource: Currently Producing {powerPlant.CurrentlyProducing():0.00} kW, " +
            $"Current Load: {powerPlant.CurrentLoad():0.00} kW, " +
            $"Battery Level: {powerPlant.CurrentBatterLevel()} %");
    }

    private async Task UpdateValues()
    {
        try
        {
            List<Task> updates = [
                charger.UpdateValues(),
                powerPlant.UpdateValues()
            ];
            await Task.WhenAll(updates);
        }
        catch (Exception e)
        {
            Console.WriteLine($"UpdateValues failed: {e}");
        }
    }

    private void UpdateState()
    {
        var carStatus = charger.CarStatus();

        switch (carStatus)
        {
            case CarState.Unknown:
                break;
            case CarState.Idle:
                _state = States.NoCar;
                break;
            case CarState.Charging:
                _state = States.Charging;
                break;
            case CarState.WaitCar:
                Console.WriteLine("State: WaitCar");
                break;
            case CarState.Complete:
                _state = States.Stopped;
                break;
            case CarState.Error:
                CheckError();
                _state = States.Stopped;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleNoCar()
    {
        // nothing to do here, wait for the car
        Console.WriteLine("Waiting for car ...");
    }

    private void HandleConnected()
    {
        // wait for charging
        // enable charging actively ? 
        Console.WriteLine("Car connected ...");
    }

    private void HandleStopped()
    {
        // wait for cable removal
        Console.WriteLine("Stopped ...");
        
        if(ShouldEnable())
        {
            Console.WriteLine("Enable");
            _ = charger.StartCharging();
        }
    }

    private void CheckError()
    {
        if (charger.Error() is null)
            return;
        
        Console.WriteLine($"Error: {charger.Error()}");
    }
    
    private async Task HandleCharging()
    {
        Console.WriteLine("Charging ...");
        
        if (ShouldIncreaseAmps())
        {
            await IncreaseAmps();
        }
        
        if (ShouldDecreaseAmps())
        {
            await DecreaseAmps();
        }

        if (ShouldDisable())
        {
            Console.WriteLine("Disable");
            _ = charger.StopCharging();
            
            _state = States.Stopped;
        }
    }
    
    private async Task IncreaseAmps()
    {
        var ampereLimit = charger.CurrentLimit();
        if (ampereLimit < MaximumAmps)
        {
            Console.WriteLine($"Increase amps by 1 to {ampereLimit + 1}");
            await charger.ChangeCurrent(1);
        }
        else
        {
            Console.WriteLine($"Cant decrease further, {ampereLimit} is maximum");
        }
    }

    private async Task DecreaseAmps()
    {
        var ampereLimit = charger.CurrentLimit();
        if (ampereLimit > _minimalAmpere)
        {
            Console.WriteLine($"Decrease amps by 1 to {ampereLimit - 1}");
            await charger.ChangeCurrent(-1);
        }
        else
        {
            Console.WriteLine($"Cant decrease further, {ampereLimit} is minimum");
        }
    }
    
    private bool ShouldIncreaseAmps()
    {
        var overProduction = powerPlant.CurrentlyProducing() - powerPlant.CurrentLoad();
        var currentSocLevel = powerPlant.CurrentBatterLevel();
        
        return overProduction > 1.0 && currentSocLevel > MinimalSocLevelEnable;
    }

    private bool ShouldDecreaseAmps()
    {
        var overProduction = powerPlant.CurrentlyProducing() - powerPlant.CurrentLoad();
        var currentSocLevel = powerPlant.CurrentBatterLevel();
        
        return overProduction < -1.0 || currentSocLevel < MinimalSocLevelDisable;
    }

    private bool ShouldEnable()
    {
        var chargedPower = charger.ChargedPower();
        var currentPower = powerPlant.CurrentlyProducing();
        var overProduction = powerPlant.CurrentlyProducing() - powerPlant.CurrentLoad();
        
        return EnoughBattery() && 
               (maximumPowerToCharge is null || chargedPower < maximumPowerToCharge) && 
               currentPower >= MinimalPowerPvEnable && 
               overProduction >= MinimalPowerPvEnable;
    }

    private bool EnoughBattery()
    {
        if(!powerPlant.HasBattery())
            return true;
        
        return powerPlant.CurrentBatterLevel() > MinimalSocLevelEnable;
    }

    private bool ShouldDisable()
    {
        var currentSocLevel = powerPlant.CurrentBatterLevel();
        var currentPower = powerPlant.CurrentlyProducing();
        
        return currentSocLevel <= MinimalSocLevelDisable || 
               currentPower <= MinimalPowerPvDisable;
    }
}

using ChargeControl.Charger;
using ChargeControl.Charger.GoeChargerApi;
using ChargeControl.PowerPlant;

namespace ChargeControl;

public class ChargeController(
    ICharger charger,
    IPowerPlant powerPlant,
    int? minimalAmps = null,
    double? maximumPowerToCharge = null,
    bool? stopped = false,
    int updateInterval = 5000)
{
    private ChargeState ChargeState = ChargeState.NoCar;
    private bool _stopped = stopped ?? false;
    
    private const double MinimalPowerPvEnable = 4.0;
    private const double MinimalPowerPvDisable = 2.5;
    private const double MinimalBatteryLevelEnable = 50.0;
    private const double MinimalBatteryLevelDisable = 30.0;

    private readonly int _minimalAmpere = minimalAmps ?? 8;
    private const int MaximumAmps = 16;
    private readonly int _updateInterval = updateInterval;

    public ChargeState GetChargeState()
    {
        return ChargeState;
    }

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
        Console.WriteLine($"Minimum Ampere: {_minimalAmpere}, Max Power: {maximumPowerToCharge}");

        do
        {
            await UpdateValues();
            PrintValues();

            UpdateState();

            switch (ChargeState)
            {
                case ChargeState.NoCar: HandleNoCar(); break;
                case ChargeState.Connected: HandleConnected(); break;
                case ChargeState.Charging: await HandleCharging(); break;
                case ChargeState.Stopped: HandleStopped(); break;
            }

            Thread.Sleep(_updateInterval);
        } while (!_stopped);
        
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
                ChargeState = ChargeState.NoCar;
                break;
            case CarState.Charging:
                ChargeState = ChargeState.Charging;
                break;
            case CarState.WaitCar:
                Console.WriteLine("State: WaitCar");
                break;
            case CarState.Complete:
                ChargeState = ChargeState.Stopped;
                break;
            case CarState.Error:
                CheckError();
                ChargeState = ChargeState.Stopped;
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
            
            ChargeState = ChargeState.Stopped;
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
        return OverProduction() > 1.0 && EnoughBattery();
    }

    private bool ShouldDecreaseAmps()
    {
        return OverProduction() < -1.0 || TooLowBattery();
    }

    private bool ShouldEnable()
    {
        return EnoughBattery() && 
               (maximumPowerToCharge is null || charger.ChargedPower() < maximumPowerToCharge) && 
               powerPlant.CurrentlyProducing() >= MinimalPowerPvEnable && 
               OverProduction() >= MinimalPowerPvEnable;
    }

    private bool EnoughBattery()
    {
        if(!powerPlant.HasBattery())
            return true;
        
        return powerPlant.CurrentBatterLevel() > MinimalBatteryLevelEnable;
    }

    private bool TooLowBattery()
    {
        if (!powerPlant.HasBattery())
            return false;
        
        return powerPlant.CurrentBatterLevel() <= MinimalBatteryLevelDisable;
    }

    private double OverProduction()
    {
        return powerPlant.CurrentlyProducing() - powerPlant.CurrentLoad();
    }

    private bool ShouldDisable()
    {
        return TooLowBattery() || 
               powerPlant.CurrentlyProducing() <= MinimalPowerPvDisable;
    }
}

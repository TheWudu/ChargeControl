using ChargeControl.Charger;
using ChargeControl.Charger.GoeChargerApi;
using ChargeControl.PowerPlant;

namespace ChargeControl;

public class ChargeController
{
    public ChargeController(
        ICharger charger,
        IPowerPlant powerPlant,
        int? minimalAmps = null,
        double? maximumPowerToCharge = null)
    {
        this.charger = charger;
        this.powerPlant = powerPlant;
        _minimalAmpere = minimalAmps ?? 8;
        _maximumPowerToCharge = maximumPowerToCharge;
    }
    private States _state = States.NoCar;
    private bool _stopped;
    
    private const double MinimalPowerPvEnable = 4.0;
    private const double MinimalPowerPvDisable = 2.5;
    private const double MinimalSocLevelEnable = 50.0;
    private const double MinimalSocLevelDisable = 30.0;

    private readonly ICharger charger;
    private readonly IPowerPlant powerPlant;
    private double? _maximumPowerToCharge;
    private int _minimalAmpere;
    private const int MaximumAmps = 16;
    private const int UpdateInterval = 5000;

    public async Task Setup()
    {
        await UpdateValues();

        // start with minimal Amps
        if (charger.AmpereLimit() != _minimalAmpere)
        {
            await charger.SetAmpereLimit(_minimalAmpere);
        }

        // set maximum power to charge if given
        if (_maximumPowerToCharge != null && !charger.PowerLimit().Equals(_maximumPowerToCharge))
        {
            await charger.SetPowerLimit((double)_maximumPowerToCharge);
        }
    }

    public async Task Run()
    {
        _stopped = false;
        
        Console.WriteLine($"Minimum Ampere: {_minimalAmpere}, Max Power: {_maximumPowerToCharge}");
        
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
                          $"PowerLimit: {charger.PowerLimit()} kWh, Current Amps: {charger.AmpereLimit()} A, " +
                          $"CarStatus: {charger.CarStatus()}");

        Console.WriteLine(
            $"PowerSource: Currently Producting {powerPlant.CurrentPowerFlowProducing():0.00} kW, " +
            $"Current Load: {powerPlant.CurrentPowerFlowLoad():0.00} kW, " +
            $"Battery Level: {powerPlant.CurrentBatterLevel()} %");
    }

    private async Task UpdateValues()
    {
        try
        {
            List<Task> updates = [
                charger.ReadValues(),
                powerPlant.ReadValues()
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
        var ampereLimit = charger.AmpereLimit();
        
        if (ShouldIncreaseAmps())
        {
            if (charger.AmpereLimit() < MaximumAmps)
            {
                // Increase Amps
                Console.WriteLine($"Increase amps by 1 to {ampereLimit + 1}");
                await charger.ChangeAmp(1);
            }
            else
            {
                Console.WriteLine($"Cant decrease further, {ampereLimit} is maximum");
            }
        }
        
        if (ShouldDecreaseAmps())
        {
            if (ampereLimit > _minimalAmpere)
            {
                // decrease Amps
                Console.WriteLine($"Decrease amps by 1 to {ampereLimit - 1}");
                await charger.ChangeAmp(-1);
            }
            else
            {
                Console.WriteLine($"Cant decrease further, {ampereLimit} is minimum");
            }
        }

        if (ShouldDisable())
        {
            Console.WriteLine("Disable");
            _ = charger.StopCharging();
            
            _state = States.Stopped;
        }
    }

    private bool ShouldIncreaseAmps()
    {
        var overProduction = powerPlant.CurrentPowerFlowProducing() - powerPlant.CurrentPowerFlowLoad();
        var currentSocLevel = powerPlant.CurrentBatterLevel();
        
        return overProduction > 1.0 && currentSocLevel > MinimalSocLevelEnable;
    }

    private bool ShouldDecreaseAmps()
    {
        var overProduction = powerPlant.CurrentPowerFlowProducing() - powerPlant.CurrentPowerFlowLoad();
        var currentSocLevel = powerPlant.CurrentBatterLevel();
        
        return overProduction < -1.0 || currentSocLevel < MinimalSocLevelDisable;
    }

    private bool ShouldEnable()
    {
        var currentSocLevel = powerPlant.CurrentBatterLevel();
        var chargedPower = charger.ChargedPower();
        var currentPower = powerPlant.CurrentPowerFlowProducing();
        var overProduction = powerPlant.CurrentPowerFlowProducing() - powerPlant.CurrentPowerFlowLoad();
        
        return currentSocLevel >= MinimalSocLevelEnable && 
               (_maximumPowerToCharge is null ? true : chargedPower < _maximumPowerToCharge) && 
               currentPower >= MinimalPowerPvEnable && 
               overProduction >= MinimalPowerPvEnable;
    }

    private bool ShouldDisable()
    {
        var currentSocLevel = powerPlant.CurrentBatterLevel();
        var currentPower = powerPlant.CurrentPowerFlowProducing();
        
        return currentSocLevel <= MinimalSocLevelDisable || 
               currentPower <= MinimalPowerPvDisable;
    }
}

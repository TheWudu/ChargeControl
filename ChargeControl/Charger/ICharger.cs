using ChargeControl.Charger.GoeChargerApi;

namespace ChargeControl.Charger;

public interface ICharger
{
    public Task<bool> UpdateValues();
    
    public Task ChangeCurrent(int changeBy);
    public Task StartCharging();
    public Task StopCharging();

    public double ChargedPower();
    public double PowerLimit();
    public int CurrentLimit();
    public CarState CarStatus();

    public Task SetCurrentLimit(int ampere);
    public Task SetPowerLimit(double limit);

    public string? Error();
}
using ChargeControl.Charger.GoeChargerApi;

namespace ChargeControl.Charger;

public interface ICharger
{
    public Task<bool> ReadValues();
    public Task ChangeAmp(int changeBy);
    public Task StartCharging();
    public Task StopCharging();

    public double ChargedPower();
    public double PowerLimit();
    public int AmpereLimit();
    public CarState CarStatus();

    public Task SetAmpereLimit(int ampere);
    public Task SetPowerLimit(double limit);

    public string? Error();
}
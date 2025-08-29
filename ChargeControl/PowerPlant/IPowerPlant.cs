namespace ChargeControl.PowerPlant;

public interface IPowerPlant
{
    public Task<bool> UpdateValues();

    public double CurrentlyProducing();
    public double CurrentLoad();
    public double CurrentPowerToGrid();
    public double CurrentPowerToBattery();

    public bool HasBattery();
    public double CurrentBatterLevel();
    public double BatteryCapacity();
}
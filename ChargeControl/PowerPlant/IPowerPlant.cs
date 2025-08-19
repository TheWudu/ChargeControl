namespace ChargeControl.PowerPlant;

public interface IPowerPlant
{
    public Task ReadValues();

    public double CurrentPowerFlowProducing();
    public double CurrentPowerFlowLoad();
    public double CurrentPowerFlowGrid();
    public double CurrentPowerFlowAkku();
    
    public double CurrentBatterLevel();
    public double BatteryCapacity();
}
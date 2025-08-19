namespace ChargeControl.PowerPlant;

public interface IPowerPlant
{
    public Task Fetch();

    public double CurrentPowerFlowProducing();
    public double CurrentPowerFlowLoad();
    public double CurrentPowerFlowGrid();
    public double CurrentPowerFlowAkku();
    
    public double SocCurrentLevel();
    public double SocCapacity();
}
using ChargeControl;
using ChargeControl.Charger;
using ChargeControl.Charger.GoeChargerApi;
using ChargeControl.PowerPlant;
using Moq;

namespace ChargeControlTests;

public class ChargeControllerTests
{
    private ChargeController _chargeController;
    private readonly Mock<ICharger> _chargerMock = new();
    private readonly Mock<IPowerPlant> _powerPlantMock = new();

    private readonly int _updateInterval = 1; // ms

    public ChargeControllerTests()
    {
        _chargeController = new ChargeController(_chargerMock.Object, _powerPlantMock.Object, null, null, true, _updateInterval);

        // mock updating away
        _chargerMock.Setup(s => s.UpdateValues()).ReturnsAsync(true);
        _powerPlantMock.Setup(s => s.UpdateValues()).ReturnsAsync(true);
        
        // mock relevant powerPlant values
        _powerPlantMock.Setup(s => s.CurrentlyProducing()).Returns(5.0);   // kWh
        _powerPlantMock.Setup(s => s.CurrentBatterLevel()).Returns(100.0); // %
        _powerPlantMock.Setup(s => s.CurrentLoad()).Returns(0.4);          // kWh
        _powerPlantMock.Setup(s => s.BatteryCapacity()).Returns(7.78);     // kWh
        
        // mock relevant charger values
        _chargerMock.Setup(s => s.CurrentLimit()).Returns(8);    // A
        _chargerMock.Setup(s => s.ChargedPower()).Returns(20.0); // kWh
        _chargerMock.Setup(s => s.CarStatus()).Returns(CarState.Charging);
    }
    
    [Fact]
    public async Task SetStateToChargingDependingOnCarState()
    {
        _chargerMock.Setup(s => s.CarStatus()).Returns(CarState.Charging);
        
        await _chargeController.Run(); 

        Assert.Equal(ChargeState.Charging, _chargeController.GetChargeState());
    }
    
    [Fact]
    public async Task SetStateToStoppedDependingOnCarState()
    {
        _chargerMock.Setup(s => s.CarStatus()).Returns(CarState.Complete);
        
        await _chargeController.Run(); 

        Assert.Equal(ChargeState.Stopped, _chargeController.GetChargeState());
    }
}
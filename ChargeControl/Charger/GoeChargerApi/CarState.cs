namespace ChargeControl.Charger.GoeChargerApi;

public enum CarState
{
    Unknown = 0, 
    Idle = 1, 
    Charging = 2, 
    WaitCar = 3, 
    Complete = 4, 
    Error = 5
}
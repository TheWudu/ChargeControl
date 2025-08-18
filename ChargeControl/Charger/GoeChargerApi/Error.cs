namespace ChargeControl.Charger.GoeChargerApi;

public enum Error
{
    None = 0, 
    FiAc = 1, 
    FiDc = 2, 
    Phase = 3, 
    Overvolt = 4, 
    Overamp = 5, 
    Diode = 6, 
    PpInvalid = 7, 
    GndInvalid = 8, 
    ContactorStuck = 9, 
    ContactorMiss = 10, 
    FiUnknown = 11, 
    Unknown = 12, 
    Overtemp = 13, 
    NoComm = 14, 
    StatusLockStuckOpen = 15, 
    StatusLockStuckLocked = 16, 
    Reserved20 = 20, 
    Reserved21 = 21, 
    Reserved22 = 22, 
    Reserved23 = 23, 
    Reserved24 = 24
}
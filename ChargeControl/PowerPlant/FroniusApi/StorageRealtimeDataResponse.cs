namespace ChargeControl.PowerPlant.FroniusApi;


// "Body" => {
//     "Data" => {
//         "0" => {
//             "Controller" => {
//                 "Capacity_Maximum" => 7680.0,
//                 "Current_DC" => 0.0,
//                 "DesignedCapacity" => 7680.0,
//                 "Details" => {
//                     "Manufacturer" => "BYD",
//                     "Model" => "BYD Battery-Box Premium HV",
//                     "Serial" => "P030T020Z2301030816     "
//                 },
//                 "Enable" => 1,
//                 "StateOfCharge_Relative" => 78.8,
//                 "Status_BatteryCell" => 3.0,
//                 "Temperature_Cell" => 23.5,
//                 "TimeStamp" => 1754712745,
//                 "Voltage_DC" => 317.3
//             },
//             "Modules" => []
//         }
//     }
// },
// "Head" => {
//     "RequestArguments" => {
//         "Scope" => "System"
//     },
//     "Status" => {
//         "Code" => 0,
//         "Reason" => "",
//         "UserMessage" => ""
//     },
//     "Timestamp" => "2025-08-09T04:12:29+00:00"
// }

public class StorageRealtimeDataResponse
{
    public required StorageRealtimeDataBody Body { get; set; }
}

public class StorageRealtimeDataBody
{
    public required Dictionary<string, StorageRealtimeData> Data { get; set; }
}

public class StorageRealtimeData
{
    public required StorageRealtimeDataController Controller { get; set; }
    // Modules
}

public class StorageRealtimeDataController
{
    public double Capacity_Maximum { get; set; }
    public double Current_DC { get; set; }
    public double DesignedCapacity { get; set; }

    public Dictionary<string, string> Details { get; set; }

    public int Enable { get; set; }
    public double StateOfCharge_Relative { get; set; }
    public double Status_BatteryCell { get; set; }
    public double Temperature_Cell { get; set; }
    public int TimeStamp { get; set; }
    public double Voltage_DC { get; set; }
}
namespace ChargeControl.Fronius;

// Get PowerFlowRealtimeData
// {
// "Body" => {
//     "Data" => {
//         "Inverters" => {
//             "1" => {
//                 "Battery_Mode" => "normal",
//                 "DT" => 1,
//                 "E_Day" => nil,
//                 "E_Total" => 19290780.198333334,
//                 "E_Year" => nil,
//                 "P" => 705.3673095703125,
//                 "SOC" => 77.9
//             }
//         },
//         "SecondaryMeters" => {},
//         "Site" => {
//             "BackupMode" => false,
//             "BatteryStandby" => false,
//             "E_Day" => nil,
//             "E_Total" => 19290780.198333334,
//             "E_Year" => nil,
//             "Meter_Location" => "grid",
//             "Mode" => "bidirectional",
//             "P_Akku" => 2.2372138500213623,
//             "P_Grid" => 255.9,
//             "P_Load" => -961.2673095703125,
//             "P_PV" => 755.5763931274414,
//             "rel_Autonomy" => 73.37889289979209,
//             "rel_SelfConsumption" => 100.0
//         },
//         "Smartloads" => {
//             "OhmpilotEcos" => {},
//             "Ohmpilots" => {}
//         },
//         "Version" => "13"
//     }
// },
// "Head" => {
//     "RequestArguments" => {},
//     "Status" => {
//         "Code" => 0,
//         "Reason" => "",
//         "UserMessage" => ""
//     },
//     "Timestamp" => "2025-08-09T04:42:21+00:00"
// }
// }
public class PowerFlowRealtimedataResponse
{
    public PowerFlowRealtimeDataBody Body { get; set; }
}

public class PowerFlowRealtimeDataBody
{
    public PowerFlowRealtimeData Data { get; set; }
}

public class PowerFlowRealtimeData
{
    public PowerFlowRealtimeDataSite Site { get; set; }
}

public class PowerFlowRealtimeDataSite
{
//             "BackupMode" => false,
//             "BatteryStandby" => false,
//             "E_Day" => nil,
//             "E_Total" => 19290780.198333334,
//             "E_Year" => nil,
//             "Meter_Location" => "grid",
//             "Mode" => "bidirectional",
    public double P_Akku { get; set; }
    public double P_Grid { get; set; }
    public double P_Load { get; set; }
    public double P_PV { get; set; }
//             "rel_Autonomy" => 73.37889289979209,
//             "rel_SelfConsumption" => 100.0

}
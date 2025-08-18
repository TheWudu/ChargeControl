using System.Text.Json.Serialization;

namespace ChargeControl.Fronius;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
public class InverterRealtimeDataResponse
{
    public InverterRealTimeDataBody Body { get; set; }

    // "Head" => {
    //     "RequestArguments" => {
    //         "Scope" => "System"
    //     },
    //     "Status" => {
    //         "Code" => 0,
    //         "Reason" => "",
    //         "UserMessage" => ""
    //     },
    //     "Timestamp" => "2025-08-08T17:05:24+00:00"
    // }
};

public class InverterRealTimeDataBody
{
    public InverterRealTimeData Data { get; set; }
}

public class InverterRealTimeData
{
    public Energy DAY_ENERGY { get; set; }
    public Energy PAC { get; set; }
    public Energy TOTAL_ENERGY { get; set; }
    public Energy YEAR_ENERGY { get; set; }
}

public class Energy
{
    public string Unit { get; set; }
    public Dictionary<string, double?> Values { get; set; }
}
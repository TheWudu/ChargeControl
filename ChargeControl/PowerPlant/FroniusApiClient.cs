using System.Text.Json;
using ChargeControl.PowerPlant.FroniusApi;

namespace ChargeControl.PowerPlant;

public class FroniusApiClient : IPowerPlant
{
    // private InverterRealtimeDataResponse? _inverterRealtimeData;
    private StorageRealtimeDataResponse? _storageRealtimeData;
    private PowerFlowRealtimedataResponse? _powerFlowRealtimeData;

    private readonly string _baseUrl = "http://192.168.178.63";
    
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task Fetch()
    {
        // await FetchInverterRealtimeData();
        await FetchStorageRealtimeData();
        await FetchPowerFlowRealtimeData();
    }
    
    // private async Task FetchInverterRealtimeData()
    // {
    //     var url = $"{_baseUrl}/solar_api/v1/GetInverterRealtimeData.cgi";
    //     var response = await new HttpClient().GetAsync(url);
    //
    //     var body = await response.Content.ReadAsStringAsync();
    //     
    //     _inverterRealtimeData = JsonSerializer.Deserialize<InverterRealtimeDataResponse>(body, _jsonOptions);
    // }
    
    private async Task FetchStorageRealtimeData()
    {
        var url = $"{_baseUrl}/solar_api/v1/GetStorageRealtimeData.cgi";
        var response = await new HttpClient().GetAsync(url);

        var body = await response.Content.ReadAsStringAsync();

        _storageRealtimeData = JsonSerializer.Deserialize<StorageRealtimeDataResponse>(body, _jsonOptions);
    }
    
    private async Task FetchPowerFlowRealtimeData()
    {
        var url = $"{_baseUrl}/solar_api/v1/GetPowerFlowRealtimeData.fcgi";
        var response = await new HttpClient().GetAsync(url);

        var body = await response.Content.ReadAsStringAsync();
        
        _powerFlowRealtimeData = JsonSerializer.Deserialize<PowerFlowRealtimedataResponse>(body, _jsonOptions);
    }
    
    // public double CurrentlyProducing()
    // {
    //     return _inverterRealtimeData?.Body.Data.PAC.Values.First().Value ?? 0.0;
    // }
    //
    // public string CurrentlyProducingUnit()
    // {
    //     return _inverterRealtimeData?.Body.Data.PAC.Unit ?? "N/A";
    // }

    public double CurrentPowerFlowProducing()
    {
        return _powerFlowRealtimeData?.Body.Data.Site.P_PV / 1000.0 ?? 0.0;
    }
    
    public double CurrentPowerFlowLoad()
    {
        return -_powerFlowRealtimeData?.Body.Data.Site.P_Load / 1000.0 ?? 0.0;
    }

    public double CurrentPowerFlowGrid()
    {
        return _powerFlowRealtimeData?.Body.Data.Site.P_Grid / 1000.0 ?? 0.0;
    }

    public double CurrentPowerFlowAkku()
    {
        return _powerFlowRealtimeData?.Body.Data.Site.P_Akku / 1000.0 ?? 0.0;
    }
    

    public double SocCurrentLevel()
    {
        return _storageRealtimeData?.Body.Data.Values.First().Controller.StateOfCharge_Relative ?? 0.0;
    }
    
    public double SocCapacity()
    {
        return _storageRealtimeData?.Body.Data.Values.First().Controller.Capacity_Maximum ?? 0.0;
    }
}
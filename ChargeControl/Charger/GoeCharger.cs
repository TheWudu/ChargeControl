using System.Net;
using System.Text.Json;
using ChargeControl.Charger.GoeChargerApi;

namespace ChargeControl.Charger;

public class GoeCharger : ICharger
{
    public ChargerResponse? Values;
    private readonly string BaseUrl = "http://192.168.178.53";

    public GoeCharger()
    {
        Values = null;
        
    }
    
    public async Task<bool> ReadValues()
    {
        try
        {
            HttpClient client = new();
            var url = $"{BaseUrl}/api/status?filter=dwo,amp,wh,acu,cus,eto,err,frc,modelState,car";
            var response = await client.GetAsync(url);
            
            Values = JsonSerializer.Deserialize<ChargerResponse>(await response.Content.ReadAsStringAsync());
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Can't read values from GoeCharger: {e}");
            return false;
        }
    }

    public double ChargedPower()
    {
        return Values?.wh / 1000.0 ?? 0.0;
    }

    public double PowerLimit()
    {
        return Values?.dwo / 1000.0 ?? 0.0;
    }
    
    public int AmpereLimit()
    {
        return Values?.amp ?? 0;
    }

    public CarState CarStatus()
    {
        return Values?.car ?? CarState.Unknown;
    }

    public async Task SetAmpereLimit(int ampere)
    {
        HttpClient client = new();
        
        var url = $"{BaseUrl}/api/set?amp={ampere}";
        
        var response = await client.GetAsync(url);
        HandleResponse(url, response);
    }

    public async Task SetPowerLimit(double limit)
    {
        HttpClient client = new();
        
        var url = $"{BaseUrl}/api/set?dwo={limit * 1000}";
        
        var response = await client.GetAsync(url);
        HandleResponse(url, response);
    }

    public string? Error()
    {
        if (Values?.err == GoeChargerApi.Error.None)
            return null;

        return Values?.err.ToString();
    }

    public async Task ChangeAmp(int changeBy)
    {
        HttpClient client = new();
        
        var newAmp = Values!.amp + changeBy;
        var url = $"{BaseUrl}/api/set?amp={newAmp}";
        
        var response = await client.GetAsync(url);
        HandleResponse(url, response);
    }

    public async Task StartCharging()
    {
        HttpClient client = new();
        
        var url = $"{BaseUrl}/api/set?frc=0"; // neutral
        
        var response = await client.GetAsync(url);
        HandleResponse(url, response);
    }
    public async Task StopCharging()
    {
        HttpClient client = new();
        
        var url = $"{BaseUrl}/api/set?frc=1"; // off
        
        var response = await client.GetAsync(url);
        HandleResponse(url, response);
    }

    private void HandleResponse(string url, HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"Unable to change status {url} => {response.Content.ReadAsStringAsync().Result}");
        }

    }
}

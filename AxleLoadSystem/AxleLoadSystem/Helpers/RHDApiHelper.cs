using BOL;
using AxleLoadSystem.Models;
namespace AxleLoadSystem.Helpers;

public interface IRHDApiHelper
{
    Task<string> GetBRTAInformationByRFID(PayloadBRTA payloadBRTA);
    Task<string> GetBRTAInformationByRegistrationNumber(PayloadBRTA payloadBRTA);
}

public class RHDApiHelper : IRHDApiHelper
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    public RHDApiHelper(ILogger<RHDApiHelper> logger, IHttpClientFactory clientFactory)
    {
        _http = clientFactory.CreateClient("BRTA_API");
        _logger = logger;
    }

    public async Task<string> GetBRTAInformationByRFID(PayloadBRTA payloadBRTA)
    {
        string info = "Initiated";
        
        try
        {
            HttpResponseMessage response = await _http.PostAsJsonAsync("public/fetch-data", new { rfid = payloadBRTA.rfid, bridgeOid = payloadBRTA.bridgeOid, username = payloadBRTA.userName, password = payloadBRTA.password });
            if (response.IsSuccessStatusCode)
            {
                info = await response.Content.ReadAsStringAsync();
            }
            else
            {
                info = "GetBRTAInformationByRFID() error: " + response.ReasonPhrase + " - Status code:" + response.StatusCode;
                _logger.LogError("GetBRTAInformationByRFID() error: " + response.ReasonPhrase + " - Status code:" + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            info = "GetBRTAInformationByRFID() exception: " + ex.Message;
            _logger.LogError("GetBRTAInformationByRFID() exception: " + ex.Message);
        }
        

        return info;
    }
    public async Task<string> GetBRTAInformationByRegistrationNumber(PayloadBRTA payloadBRTA)
    {
        string info = "Initiated";
        
        try
        {
            HttpResponseMessage response = await _http.PostAsJsonAsync("public/fetch-data", new
            {
                zone = payloadBRTA.zone,
                series = payloadBRTA.series,
                number = payloadBRTA.Number,
                bridgeOid = payloadBRTA.bridgeOid,
                username = payloadBRTA.userName,
                password = payloadBRTA.password
            });
            if (response.IsSuccessStatusCode)
            {
                info = await response.Content.ReadAsStringAsync();
            }
            else
            {
                info = "GetBRTAInformationByRegistrationNumber() error: " + response.ReasonPhrase + " - Status code:" + response.StatusCode;
                _logger.LogError("GetBRTAInformationByRegistrationNumber() error: " + response.ReasonPhrase + " - Status code:" + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            info = "GetBRTAInformationByRegistrationNumber() exception: " + ex.Message;
            _logger.LogError("GetBRTAInformationByRegistrationNumber() exception: " + ex.Message);
        }

        return info;
    }
}

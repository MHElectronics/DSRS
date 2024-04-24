using BOL;
using AxleLoadSystem.Models;
namespace AxleLoadSystem.Helpers;

public interface IRHDApiHelper
{
    Task<string> GetBRTAInformationByRFID(PayloadBRTA payloadBRTA);
    Task<Brta> GetBRTAInformationByRegistrationNumber(PayloadBRTA payloadBRTA);
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
        HttpResponseMessage response = await _http.PostAsJsonAsync("public/fetch-data", new { rfid = payloadBRTA.rfid, bridgeOid = payloadBRTA.bridgeOid, username = payloadBRTA.userName, password = payloadBRTA.password });

        if (response.IsSuccessStatusCode)
        {
            info = await response.Content.ReadAsStringAsync();
        }
        else
        {
            info = "GetBRTAInformation() error: " + response.ReasonPhrase;
            _logger.LogError("GetBRTAInformation() error: " + response.ReasonPhrase);
        }

        return info;
    }
    public async Task<Brta> GetBRTAInformationByRegistrationNumber(PayloadBRTA payloadBRTA)
    {
        Brta info = new Brta();
        HttpResponseMessage response = await _http.PostAsJsonAsync("public/fetch-data", new { zone = payloadBRTA.zone,
            series=payloadBRTA.series, number=payloadBRTA.Number, bridgeOid = payloadBRTA.bridgeOid, username = payloadBRTA.userName, password = payloadBRTA.password });

        if (response.IsSuccessStatusCode)
        {
            info = await response.Content.ReadFromJsonAsync<Brta>();
        }
        else
        {
            info.message = "GetBRTAInformation() error: " + response.ReasonPhrase;
            _logger.LogError("GetBRTAInformation() error: " + response.ReasonPhrase);
        }

        return info;
    }
}

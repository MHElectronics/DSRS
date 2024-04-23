namespace AxleLoadSystem.Helpers;

public interface IRHDApiHelper
{
    Task<string> GetBRTAInformation();
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

    public async Task<string> GetBRTAInformation()
    {
        string info = "Initiated";
        HttpResponseMessage response = await _http.PostAsJsonAsync("public/fetch-data", new { rfid = "42525441FF021A151D6A0001", bridgeOid = "3", username = "", password = "" });

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
}

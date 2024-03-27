using BOL;
using Microsoft.Extensions.Configuration;
using Services.Helpers;

namespace Services;
public interface IStationService
{
    Task<IEnumerable<Station>> Get();
    Task<Station> GetById(Station obj);
    Task<bool> Add(Station obj);
    Task<bool> Update(Station obj);
    string GenerateKey();
}
public class StationService : IStationService
{
    private static Random random = new Random();
    private ISqlDataAccess _db { get; set; }
    private IConfiguration _configuration { get; set; }
    public StationService(ISqlDataAccess db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<IEnumerable<Station>> Get()
    {
        string query = "SELECT StationId,StationCode,StationName,Address,AuthKey FROM Station";
        return await _db.LoadData<Station, dynamic>(query, null);
    }

    public async Task<Station> GetById(Station obj)
    {
        string sql = "SELECT StationId,StationCode,StationName,Address,AuthKey FROM Station WHERE StationId=@StationId";
        return await _db.LoadSingleAsync<Station, dynamic>(sql, new { obj.StationId });
    }

    public async Task<bool> Add(Station obj)
    {
        int count = await _db.Insert<Station>(obj);
        return count > 0;
    }
    public async Task<bool> Update(Station obj)
    {
        string query = "UPDATE Station SET StationCode=@StationCode,StationName=@StationName,Address=@Address,AuthKey=@AuthKey WHERE StationId=@StationId";
        return await _db.SaveData<Station>(query, obj);
    }
    public async Task<bool> Delete(Station obj)
    {
        string query = "DELETE FROM Station WHERE StationId=@StationId";
        int count = await _db.DeleteData<Station, object>(query, new { obj.StationId });
        return count > 0;
    }
    public string GenerateKey()
    {
        int length = Convert.ToInt16(_configuration.GetSection("Settings:ApiKeyLength").Value);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
}

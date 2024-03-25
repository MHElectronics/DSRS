using BOL;
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
    private readonly ISqlDataAccess _db;
    public StationService(ISqlDataAccess db)
    {
        _db = db;
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
    public Task<bool> Update(Station obj)
    {
        throw new NotImplementedException();
    }

    public string GenerateKey()
    {
        int length = 20;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
}

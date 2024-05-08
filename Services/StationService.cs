using BOL;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Services.Helpers;


namespace Services;
public interface IStationService
{
    Task<IEnumerable<Station>> Get();
    Task<Station> GetById(Station obj);
    Task<bool> Add(Station obj);
    Task<bool> Update(Station obj);
    Task<bool> Delete(Station obj);
    string GenerateKey();
}
public class StationService : IStationService
{
    private static Random random = new Random();
    private ISqlDataAccess _db { get; set; }
    private IConfiguration _configuration { get; set; }
    private IMemoryCache _cacheProvider { get; set; }
    public StationService(ISqlDataAccess db, IConfiguration configuration, IMemoryCache cacheProvider)
    {
        _db = db;
        _configuration = configuration;
        _cacheProvider = cacheProvider;
    }

    public async Task<IEnumerable<Station>> Get()
    {
        if (!_cacheProvider.TryGetValue(CacheKeys.Stations, out IEnumerable<Station> stations))
        {
            // Get the data from database
            string query = "SELECT StationId,StationCode,StationName,Address,AuthKey,MapX,MapY FROM Stations";
            stations = await _db.LoadData<Station, dynamic>(query, null);
            
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(2),
                SlidingExpiration = TimeSpan.FromMinutes(1),
                Size = 1024,
            };
            _cacheProvider.Set(CacheKeys.Stations, stations, cacheEntryOptions);
        }

        return stations;
    }
    public async Task<Station> GetById(Station obj)
    {
        if (_cacheProvider.TryGetValue(CacheKeys.Stations, out IEnumerable<Station> stations))
        {
            return stations.FirstOrDefault(s => s.StationId == obj.StationId);
        }

        string sql = "SELECT StationId,StationCode,StationName,Address,AuthKey,MapX,MapY FROM Stations WHERE StationId=@StationId";
        Station station = await _db.LoadSingleAsync<Station, dynamic>(sql, new { obj.StationId });

        //Reset cache without waiting
        if (station is not null)
        {
            this.Get();
        }

        return station;
    }

    public async Task<bool> Add(Station obj)
    {
        string query = "INSERT INTO Stations(StationCode,StationName,Address,AuthKey,MapX,MapY) VALUES(@StationCode,@StationName,@Address,@AuthKey,@MapX,@MapY)";
        bool isSuccess = await _db.SaveData<Station>(query, obj);

        //Reset cache without waiting
        if (isSuccess)
        {
            this.Get();
        }

        return isSuccess;
    }
    public async Task<bool> Update(Station obj)
    {
        string query = "UPDATE Stations SET StationCode=@StationCode,StationName=@StationName,Address=@Address,AuthKey=@AuthKey,MapX=@MapX,MapY=@MapY WHERE StationId=@StationId";
        bool isSuccess = await _db.SaveData<Station>(query, obj);
        
        //Reset cache without waiting
        if (isSuccess)
        {
            this.Get();
        }

        return isSuccess;
    }
    public async Task<bool> Delete(Station obj)
    {
        string query = "DELETE FROM WIMScale WHERE StationId=@StationId " + 
                       "DELETE FROM Stations WHERE StationId=@StationId";
        int count = await _db.DeleteData<Station, object>(query, new { obj.StationId });

        //Reset cache without waiting
        if (count > 0)
        {
            this.Get();
        }

        return count > 0;
    }
    
    public string GenerateKey()
    {
        int length = Convert.ToInt16(_configuration.GetSection("Settings:ApiKeyLength").Value);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

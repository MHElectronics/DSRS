using BOL;
using Microsoft.Extensions.Caching.Memory;
using Services.Helpers;

namespace Services;
public interface IWIMScaleService
{
    Task<IEnumerable<WIMScale>> GetAll();
    Task<IEnumerable<WIMScale>> GetByStation(WIMScale obj);
    Task<IEnumerable<WIMScale>> GetByStationId(int stationId);
    Task<WIMScale> GetById(WIMScale obj);
    Task<bool> Add(WIMScale obj, User user);
    Task<bool> Update(WIMScale obj, User user);
    Task<bool> Delete(WIMScale obj, User user);
}
public class WIMScaleService : IWIMScaleService
{
    private IMemoryCache _cacheProvider { get; set; }
    private ISqlDataAccess _db { get; set; }
    private readonly IUserActivityService _userActivityService;
    public WIMScaleService(ISqlDataAccess db, IMemoryCache cacheProvider, IUserActivityService userActivityService)
    {
        _db = db;
        _cacheProvider = cacheProvider;
        _userActivityService = userActivityService;
    }

    public async Task<IEnumerable<WIMScale>> GetAll()
    {
        // Get the data from database
        string query = "SELECT * FROM WIMScale";
        return await _db.LoadData<WIMScale, dynamic>(query, null);
    }

    public async Task<IEnumerable<WIMScale>> GetByStation(WIMScale obj)
    {
        string cacheKey = "WIMS_S_" + obj.StationId;
        
        if (!_cacheProvider.TryGetValue(cacheKey, out IEnumerable<WIMScale>? wims))
        {
            // Get the data from database
            string query = "SELECT * FROM WIMScale WHERE StationId=@StationId";
            wims = await _db.LoadData<WIMScale, dynamic>(query, new { obj.StationId });
            
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(2),
                SlidingExpiration = TimeSpan.FromMinutes(1),
                Size = 1024,
            };
            _cacheProvider.Set(cacheKey, wims, cacheEntryOptions);
        }

        return wims;
    }
    public async Task<IEnumerable<WIMScale>> GetByStationId(int stationId)
    {
        string query = "SELECT * FROM WIMScale WHERE StationId=@stationId";
        return await _db.LoadData<WIMScale, dynamic>(query, new { stationId });
    }

    public async Task<WIMScale> GetById(WIMScale obj)
    {
        string sql = "SELECT * FROM WIMScale WHERE Id=@Id";
        return await _db.LoadSingleAsync<WIMScale, dynamic>(sql, new { obj.Id });
    }

    public async Task<bool> Add(WIMScale obj, User user)
    {
        bool hasDuplicate = await this.CheckDuplicateEntry(obj);
        if (!hasDuplicate)
        {
            string query = "INSERT INTO WIMScale(StationId,LaneNumber,Type,EquipmentCode,LaneDirection,IsUpbound) VALUES(@StationId,@LaneNumber,@Type,@EquipmentCode,@LaneDirection,@IsUpbound)";
            bool isSuccess = await _db.SaveData<WIMScale>(query, obj);
            if(isSuccess)
            {
                string cacheKey = "WIMS_S_" + obj.StationId;
                _cacheProvider.Remove(cacheKey);

                UserActivity log = new UserActivity(user.Id, "Station:" + obj.StationId + " Lane: " + obj.LaneNumber +" Added", LogActivity.Insert);
                await _userActivityService.InsertUserActivity(log);
            }
            return isSuccess;
        }
        return false;
    }
    public async Task<bool> Update(WIMScale obj, User user)
    {
        string query = "UPDATE WIMScale SET StationId=@StationId,LaneNumber=@LaneNumber,Type=@Type,EquipmentCode=@EquipmentCode,LaneDirection=@LaneDirection,IsUpbound=@IsUpbound WHERE Id=@Id";
        bool isSuccess = await _db.SaveData<WIMScale>(query, obj);
        if (isSuccess)
        {
            string cacheKey = "WIMS_S_" + obj.StationId;
            _cacheProvider.Remove(cacheKey);

            UserActivity log = new UserActivity(user.Id, "Station:" + obj.StationId + " Lane: " + obj.LaneNumber + " Updated", LogActivity.Update);
            await _userActivityService.InsertUserActivity(log);
        }
        return isSuccess;
    }
    public async Task<bool> Delete(WIMScale obj, User user)
    {
        string query = "DELETE FROM WIMScale WHERE Id=@Id";
        int count = await _db.DeleteData<WIMScale, object>(query, new { obj.Id });
        if(count > 0)
        {
            string cacheKey = "WIMS_S_" + obj.StationId;
            _cacheProvider.Remove(cacheKey);

            UserActivity log = new UserActivity(user.Id, "Station:" + obj.StationId + " Lane: " + obj.LaneNumber + " Deleted", LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);

            return true;
        }

        return false;
    }
    public async Task<bool> CheckDuplicateEntry(WIMScale wimScale)
    {
        string query = "SELECT COUNT(1) Count FROM WIMScale WHERE LaneNumber=@LaneNumber AND StationId=@StationId";
        return await _db.LoadSingleAsync<bool, object>(query, wimScale);
    }
}

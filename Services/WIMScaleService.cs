using BOL;
using Microsoft.Extensions.Caching.Memory;
using Services.Helpers;

namespace Services;
public interface IWIMScaleService
{
    Task<IEnumerable<WIMScale>> Get(WIMScale obj);
    Task<WIMScale> GetById(WIMScale obj);
    Task<bool> Add(WIMScale obj);
    Task<bool> Update(WIMScale obj);
    Task<bool> Delete(WIMScale obj);
}
public class WIMScaleService : IWIMScaleService
{
    private IMemoryCache _cacheProvider { get; set; }
    private ISqlDataAccess _db { get; set; }
    public WIMScaleService(ISqlDataAccess db, IMemoryCache cacheProvider)
    {
        _db = db;
        _cacheProvider = cacheProvider;
    }

    public async Task<IEnumerable<WIMScale>> Get(WIMScale obj)
    {
        string cacheKey = "WIMS";
        if(obj.StationId > 0)
        {
            cacheKey = "WIMS_S_" + obj.StationId;
        }
        
        if (!_cacheProvider.TryGetValue(cacheKey, out IEnumerable<WIMScale> wims))
        {
            // Get the data from database
            string query = "SELECT Id,StationId,LaneNumber,IsHighSpeed,EquipmentCode,LaneDirection FROM WIMScale WHERE 1=1";
            Dictionary<string, object> param = new Dictionary<string, object>();
            if (obj.StationId > 0)
            {
                query += " AND StationId=@StationId";
                param.Add("@StationId", obj.StationId);
            }
            wims = await _db.LoadData<WIMScale, dynamic>(query, param);
            
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(2),
                SlidingExpiration = TimeSpan.FromMinutes(1),
                Size = 1024,
            };
            _cacheProvider.Set(cacheKey, wims, cacheEntryOptions);
        }

        return wims;

        //string query = "SELECT Id,StationId,LaneNumber,IsHighSpeed,EquipmentCode,LaneDirection FROM WIMScale WHERE 1=1";
        //Dictionary<string, object> param = new Dictionary<string, object>();
        //if(obj.StationId > 0)
        //{
        //    query += " AND StationId=@StationId";
        //    param.Add("@StationId", obj.StationId);
        //}
        //return await _db.LoadData<WIMScale, dynamic>(query, param);
    }

    public async Task<WIMScale> GetById(WIMScale obj)
    {
        string sql = "SELECT Id,StationId,LaneNumber,IsHighSpeed,EquipmentCode,LaneDirection FROM WIMScale WHERE Id=@Id";
        return await _db.LoadSingleAsync<WIMScale, dynamic>(sql, new { obj.Id });
    }

    public async Task<bool> Add(WIMScale obj)
    {
        string query = "INSERT INTO WIMScale(StationId,LaneNumber,IsHighSpeed,EquipmentCode,LaneDirection) VALUES(@StationId,@LaneNumber,@IsHighSpeed,@EquipmentCode,@LaneDirection)";
        return await _db.SaveData<WIMScale>(query, obj);
    }
    public async Task<bool> Update(WIMScale obj)
    {
        string query = "UPDATE WIMScale SET StationId=@StationId,LaneNumber=@LaneNumber,IsHighSpeed=@IsHighSpeed,EquipmentCode=@EquipmentCode,LaneDirection=@LaneDirection WHERE Id=@Id";
        return await _db.SaveData<WIMScale>(query, obj);
    }
    public async Task<bool> Delete(WIMScale obj)
    {
        string query = "DELETE FROM WIMScale WHERE Id=@Id";
        int count = await _db.DeleteData<WIMScale, object>(query, new { obj.Id });
        return count > 0;
    }
}

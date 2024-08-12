using BOL;
using Services.Helpers;

namespace Services;

public interface IStationAccessService
{
    Task<StationAccess> GetStationAccessByUserId(int userId);
    Task<IEnumerable<StationAccess>> GetStationAccesses();
    Task<bool> InsertStationAccess(StationAccess stationAccess);
    Task<bool> UpdateStationAccess(StationAccess stationAccess);
    Task<bool> Delete(StationAccess stationAccess);

}
public class StationAccessService(ISqlDataAccess _db) : IStationAccessService
{
    public async Task<bool> Delete(StationAccess stationAccess)
    {
        string query = "DELETE FROM StationAccess WHERE Id=@Id";
        int count = await _db.DeleteData<StationAccess, object>(query, new { stationAccess.Id });
        return count > 0;
    }

    public async Task<StationAccess> GetStationAccessByUserId(int userId)
    {
        return await _db.LoadSingleAsync<StationAccess, dynamic>("SELECT * FROM StationAccess WHERE UserId=@UserId", new { UserId = userId });
    }

    public async Task<IEnumerable<StationAccess>> GetStationAccesses() => 
        await _db.LoadData<StationAccess, dynamic>("SELECT * FROM StationAccess", new { });

    public async Task<bool> InsertStationAccess(StationAccess stationAccess)
    {
        stationAccess.EntryTime = DateTime.Now;
        bool hasDuplicate = await this.CheckDuplicateEntry(stationAccess);
        if (!hasDuplicate) 
        {
            string sql = @"INSERT INTO StationAccess(UserId,StationIds,EntryTime,EntryBy)
            VALUES (@UserId,@StationIds,@EntryTime,@EntryBy)";
            return await _db.SaveData<StationAccess>(sql, stationAccess);
        }
        return false;
    }

    public async Task<bool> UpdateStationAccess(StationAccess stationAccess)
    {
        string sql = @"UPDATE StationAccess SET UserId=@UserId, StationIds=@StationIds, EntryBy=@EntryBy WHERE Id=@Id";
        return await _db.SaveData(sql, stationAccess);
    }

    public async Task<bool> CheckDuplicateEntry(StationAccess stationAccess)
    {
        string query = "SELECT COUNT(1) Count FROM StationAccess WHERE UserId=@UserId";
        return await _db.LoadSingleAsync<bool, object>(query, stationAccess);
    }
}

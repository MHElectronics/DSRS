using BOL;
using Services.Helpers;

namespace Services;

public interface IStationAccessService
{
    Task<IEnumerable<Station>> GetStationAccessByUserId(int userId);
    Task<IEnumerable<StationAccess>> GetStationAccessByStationId(int stationId);
    Task<IEnumerable<StationAccess>> GetStationAccesses();
    Task<bool> InsertStationAccess(StationAccess stationAccess);
    Task<bool> InsertStationAccess(List<StationAccess> stationAccessList);
    Task<bool> Delete(StationAccess stationAccess);
    Task<bool> DeleteStationAccessByUserId(int userId);
    Task<bool> DeleteStationAccessByStationId(int stationId);

}
public class StationAccessService(ISqlDataAccess _db) : IStationAccessService
{
    public async Task<bool> Delete(StationAccess stationAccess)
    {
        string query = "DELETE FROM StationAccess WHERE Id=@Id";
        int count = await _db.DeleteData<StationAccess, object>(query, new { stationAccess.Id });
        return count > 0;
    }

    public async Task<IEnumerable<Station>> GetStationAccessByUserId(int userId)
    {
        string sql = "SELECT S.* FROM Stations S INNER JOIN StationAccess SA ON S.StationId=SA.StationId WHERE UserId=@UserId";
        return await _db.LoadData<Station, object>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<StationAccess>> GetStationAccessByStationId(int stationId)
    {
        string sql = "SELECT * FROM StationAccess WHERE StationId=@StationId";
        return await _db.LoadData<StationAccess, dynamic>(sql, new { StationId = stationId });
    }

    public async Task<IEnumerable<StationAccess>> GetStationAccesses() =>
        await _db.LoadData<StationAccess, dynamic>("SELECT * FROM StationAccess", new { });

    public async Task<bool> InsertStationAccess(StationAccess stationAccess)
    {
        stationAccess.EntryTime = DateTime.Now;
        bool hasDuplicate = await this.CheckDuplicateEntry(stationAccess);
        if (!hasDuplicate)
        {
            string sql = @"INSERT INTO StationAccess(UserId,StationId,EntryTime,EntryBy)
            VALUES (@UserId,@StationId,@EntryTime,@EntryBy)";
            return await _db.SaveData<StationAccess>(sql, stationAccess);
        }
        return false;
    }
    public async Task<bool> InsertStationAccess(List<StationAccess> stationAccessList)
    {
        string sql = @"INSERT INTO StationAccess(UserId,StationId,EntryTime,EntryBy)
            VALUES (@UserId,@StationId,@EntryTime,@EntryBy)";

        return await _db.SaveData<List<StationAccess>>(sql, stationAccessList);

    }

    public async Task<bool> CheckDuplicateEntry(StationAccess stationAccess)
    {
        string query = "SELECT COUNT(1) Count FROM StationAccess WHERE UserId=@UserId AND StationId=@StationId";
        return await _db.LoadSingleAsync<bool, object>(query, stationAccess);
    }

    public async Task<bool> DeleteStationAccessByUserId(int UserId)
    {
        string query = "DELETE FROM StationAccess WHERE UserId=@UserId";
        int count = await _db.DeleteData<StationAccess, dynamic>(query, new { UserId });
        return count > 0;
    }

    public async Task<bool> DeleteStationAccessByStationId(int StationId)
    {
        string query = "DELETE FROM StationAccess WHERE StationId=@StationId";
        int count = await _db.DeleteData<StationAccess, dynamic>(query, new { StationId });
        return count > 0;
    }
}

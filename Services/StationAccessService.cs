using BOL;
using Services.Helpers;

namespace Services;

public interface IStationAccessService
{
    Task<IEnumerable<StationAccess>> GetStationAccessByUserId(int userId);
    Task<IEnumerable<StationAccess>> GetStationAccesses();
    Task<bool> InsertStationAccess(StationAccess stationAccess);
    Task<bool> InsertStationAccess(List<StationAccess> stationAccessList);
    Task<bool> Delete(StationAccess stationAccess);
    Task<bool> DeleteStationAccessByUserId(int userId);

}
public class StationAccessService(ISqlDataAccess _db) : IStationAccessService
{
    public async Task<bool> Delete(StationAccess stationAccess)
    {
        string query = "DELETE FROM StationAccess WHERE Id=@Id";
        int count = await _db.DeleteData<StationAccess, object>(query, new { stationAccess.Id });
        return count > 0;
    }

    public async Task<IEnumerable<StationAccess>> GetStationAccessByUserId(int userId)
    {
        string sql = "SELECT * FROM StationAccess WHERE UserId=@UserId";
        return await _db.LoadData<StationAccess, dynamic>(sql, new { UserId = userId });
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
        string sql = @"INSERT INTO StationAccess(UserId,StationId,EntryBy)
            VALUES (@UserId,@StationId,@EntryBy)";

        return await _db.SaveData<List<StationAccess>>(sql, stationAccessList);

        return false;
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
}

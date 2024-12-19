using BOL;
using Services.Helpers;

namespace Services;

public interface IStationAccessService
{
    Task<IEnumerable<Station>> GetStationAccessByUserId(int userId);
    Task<IEnumerable<StationAccess>> GetStationAccessByStationId(int stationId);
    Task<IEnumerable<StationAccess>> GetStationAccesses();
    Task<bool> InsertStationAccess(StationAccess stationAccess, User user);
    Task<bool> InsertStationAccess(List<StationAccess> stationAccessList, User user);
    Task<bool> Delete(StationAccess stationAccess, User user);
    Task<bool> DeleteStationAccessByUserId(int userId, User user);
    Task<bool> DeleteStationAccessByStationId(int stationId, User user);

}
public class StationAccessService(ISqlDataAccess _db, IUserActivityService _userActivityService) : IStationAccessService
{
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

    public async Task<bool> InsertStationAccess(StationAccess stationAccess, User user)
    {
        stationAccess.EntryTime = DateTime.Now;
        bool hasDuplicate = await this.CheckDuplicateEntry(stationAccess);
        if (!hasDuplicate)
        {
            string sql = @"INSERT INTO StationAccess(UserId,StationId,EntryTime,EntryBy)
            VALUES (@UserId,@StationId,@EntryTime,@EntryBy)";
            bool isSuccess = await _db.SaveData<StationAccess>(sql, stationAccess);

            if (isSuccess)
            {
                UserActivity log = new UserActivity(user.Id, "Station access added. User: " + stationAccess.UserId + " Station: " + stationAccess.StationId, LogActivity.Insert);
                await _userActivityService.InsertUserActivity(log);
            }

            return isSuccess;
        }
        return false;
    }
    public async Task<bool> InsertStationAccess(List<StationAccess> stationAccessList, User user)
    {
        string sql = @"INSERT INTO StationAccess(UserId,StationId,EntryTime,EntryBy)
            VALUES (@UserId,@StationId,@EntryTime,@EntryBy)";

        bool isSuccess = await _db.SaveData<List<StationAccess>>(sql, stationAccessList);

        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "Station access added. User: " + stationAccessList.FirstOrDefault().UserId + " Station: " + String.Join(",", stationAccessList.Select(s => s.StationId)), LogActivity.Insert);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }

    private async Task<bool> CheckDuplicateEntry(StationAccess stationAccess)
    {
        string query = "SELECT COUNT(1) Count FROM StationAccess WHERE UserId=@UserId AND StationId=@StationId";
        return await _db.LoadSingleAsync<bool, object>(query, stationAccess);
    }

    public async Task<bool> DeleteStationAccessByUserId(int userId, User user)
    {
        string query = "DELETE FROM StationAccess WHERE UserId=@UserId";
        int count = await _db.DeleteData<StationAccess, dynamic>(query, new { userId });

        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, "All Station access deleted for UserId: " + userId, LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }

        return count > 0;
    }

    public async Task<bool> DeleteStationAccessByStationId(int stationId, User user)
    {
        string query = "DELETE FROM StationAccess WHERE StationId=@StationId";
        int count = await _db.DeleteData<StationAccess, dynamic>(query, new { stationId });

        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, "All user access deleted for StationId: " + stationId, LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }

        return count > 0;
    }

    public async Task<bool> Delete(StationAccess stationAccess, User user)
    {
        string query = "DELETE FROM StationAccess WHERE Id=@Id";
        int count = await _db.DeleteData<StationAccess, object>(query, new { stationAccess.Id });

        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, "Station access deleted for StationId: " + stationAccess.StationId + " and UserId: " + stationAccess.UserId, LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }

        return count > 0;
    }
}

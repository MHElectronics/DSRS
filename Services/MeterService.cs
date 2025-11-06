using BOL;
using Services.Helpers;
namespace Services;
public interface IMeterService
{
    Task<IEnumerable<Meter>> GetMeterList();
    Task<Meter> GetMeterById(int id);
    Task<bool> InsertMeter(Meter Meter, User user);
    Task<Meter> UpdateMeter(Meter Meter, User user);
    Task<bool> DeleteMeter(int id, User user);

}
public class MeterService : IMeterService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;

    public MeterService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
    }
    public async Task<Meter> GetMeterById(int id)
    {
        IEnumerable<Meter>? results = await _db.LoadData<Meter, dynamic>("SELECT * FROM Meter WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }
    public async Task<IEnumerable<Meter>> GetMeterList() =>
        await _db.LoadData<Meter, dynamic>("SELECT * FROM Meter", new { });

    public async Task<bool> InsertMeter(Meter Meter, User user)
    {
        bool hasDuplicate = await this.CheckDuplicateMeter(Meter);
        if (!hasDuplicate)
        {
            string sql = @"INSERT INTO Meter(MeterNumber,MeterAddress,MeterType,CompanyId,DCUId,CustomerId,IsActive) VALUES(@MeterNumber,@MeterAddress,@MeterType,@CompanyId,@DCUId,@CustomerId,@IsActive)";
            bool isSuccess = await _db.SaveData<Meter>(sql, Meter);

            if (isSuccess)
            {
                UserActivity log = new UserActivity(user.Id, Meter.MeterNumber + " Meter Added", LogActivity.Insert);
                await _userActivityService.InsertUserActivity(log);
            }

            return isSuccess;
        }
        return false;
    }

    public async Task<Meter> UpdateMeter(Meter Meter, User user)
    {
        string sql = @"UPDATE Meter SET MeterNumber=@MeterNumber, MeterAddress=@MeterAddress, MeterType=@MeterType, CompanyId=@CompanyId, DCUId=@DCUId, CustomerId=@CustomerId, IsActive=@IsActive  WHERE Id=@Id";
        await _db.SaveData(sql, Meter);

        UserActivity log = new UserActivity(user.Id, Meter.MeterNumber + " Meter Updated", LogActivity.Update);
        await _userActivityService.InsertUserActivity(log);

        return Meter;
    }
    public async Task<bool> DeleteMeter(int id, User user)
    {
        string query = "DELETE FROM Meter WHERE Id=@Id";
        int count = await _db.DeleteData<Meter, object>(query, new { id });

        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, id + " Meter Delete", LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }
        return count > 0;
    }


    private async Task<bool> CheckDuplicateMeter(Meter Meter)
    {
        string query = "SELECT COUNT(1) Count FROM Meter WHERE(LOWER(MeterNumber)=LOWER(@MeterNumber) OR Id=@Id)";
        return await _db.LoadSingleAsync<bool, object>(query, Meter);
    }
}

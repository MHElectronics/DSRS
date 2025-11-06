using BOL;
using Services.Helpers;
namespace Services;
public interface IDCUService
{
    Task<IEnumerable<DCU>> GetDCUList();
    Task<DCU> GetDCUById(int id);
    Task<bool> InsertDCU(DCU DCU, User user);
    Task<DCU> UpdateDCU(DCU DCU, User user);
    Task<bool> DeleteDCU(int id, User user);

}
public class DCUService : IDCUService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;

    public DCUService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
    }
    public async Task<DCU> GetDCUById(int id)
    {
        IEnumerable<DCU>? results = await _db.LoadData<DCU, dynamic>("SELECT * FROM DCU WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }
    public async Task<IEnumerable<DCU>> GetDCUList() =>
        await _db.LoadData<DCU, dynamic>("SELECT * FROM DCU", new { });

    public async Task<bool> InsertDCU(DCU DCU, User user)
    {
        bool hasDuplicate = await this.CheckDuplicateDCU(DCU);
        if (!hasDuplicate)
        {
            string sql = @"INSERT INTO DCU(DCUNumber,DCUAddress,DCUType,CompanyId,DCUId,CustomerId,IsActive) VALUES(@DCUNumber,@DCUAddress,@DCUType,@CompanyId,@DCUId,@CustomerId,@IsActive)";
            bool isSuccess = await _db.SaveData<DCU>(sql, DCU);

            if (isSuccess)
            {
                UserActivity log = new UserActivity(user.Id, DCU.DCUNumber + " DCU Added", LogActivity.Insert);
                await _userActivityService.InsertUserActivity(log);
            }

            return isSuccess;
        }
        return false;
    }

    public async Task<DCU> UpdateDCU(DCU DCU, User user)
    {
        string sql = @"UPDATE DCU SET DCUNumber=@DCUNumber, DCUAddress=@DCUAddress, DCUType=@DCUType, CompanyId=@CompanyId, DCUId=@DCUId, CustomerId=@CustomerId, IsActive=@IsActive  WHERE Id=@Id";
        await _db.SaveData(sql, DCU);

        UserActivity log = new UserActivity(user.Id, DCU.DCUNumber + " DCU Updated", LogActivity.Update);
        await _userActivityService.InsertUserActivity(log);

        return DCU;
    }
    public async Task<bool> DeleteDCU(int id, User user)
    {
        string query = "DELETE FROM DCU WHERE Id=@Id";
        int count = await _db.DeleteData<DCU, object>(query, new { id });

        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, id + " DCU Delete", LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }
        return count > 0;
    }


    private async Task<bool> CheckDuplicateDCU(DCU DCU)
    {
        string query = "SELECT COUNT(1) Count FROM DCU WHERE(LOWER(DCUNumber)=LOWER(@DCUNumber) OR Id=@Id)";
        return await _db.LoadSingleAsync<bool, object>(query, DCU);
    }
}

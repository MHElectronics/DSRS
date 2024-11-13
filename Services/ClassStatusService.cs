using BOL;
using Services.Helpers;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Services;
public interface IClassStatusService
{
    Task<IEnumerable<ClassStatus>> GetClassStatusList();
    Task<ClassStatus> GetClassStatus(int id);
    Task<bool> InsertClassStatus(ClassStatus classStatus, User user);
    Task<ClassStatus> UpdateClassStatus(ClassStatus classStatus, User user);
    Task<bool> DeleteClassStatus(int id, User user);
}
public class ClassStatusService : IClassStatusService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;
    public ClassStatusService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
    }

    public async Task<ClassStatus> GetClassStatus(int id)
    {
        var results = await _db.LoadData<ClassStatus, dynamic>("SELECT * FROM ClassStatus WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<ClassStatus>> GetClassStatusList() =>
        await _db.LoadData<ClassStatus, dynamic>("SELECT * FROM ClassStatus", new { });

    public async Task<bool> InsertClassStatus(ClassStatus classStatus, User user)
    {
        bool hasDuplicate = await this.CheckDuplicateClassStatus(classStatus);
        if (!hasDuplicate)
        {
            string sql = @"INSERT INTO ClassStatus(Id,Name) VALUES(@Id,@Name)";
            bool isSuccess = await _db.SaveData<ClassStatus>(sql, classStatus);

            if (isSuccess)
            {
                UserActivity log = new UserActivity(user.Id, "Class Status Added", LogActivity.Insert);
                await _userActivityService.InsertUserActivity(log);
            }

            return isSuccess;
        }
        return false;
    }

    public async Task<ClassStatus> UpdateClassStatus(ClassStatus classStatus, User user)
    {
        string sql = @"UPDATE ClassStatus SET Name=@Name WHERE Id=@Id";
        await _db.SaveData(sql, classStatus);

        UserActivity log = new UserActivity(user.Id, "Class Status Updated", LogActivity.Update);
        await _userActivityService.InsertUserActivity(log);

        return classStatus;
    }
    public async Task<bool> DeleteClassStatus(int id, User user)
    {
        string query = "DELETE FROM ClassStatus WHERE Id=@Id";
        int count = await _db.DeleteData<ClassStatus, object>(query, new { id });

        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, "Class Status Delete", LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);

        }
        return count > 0;
    }


    private async Task<bool> CheckDuplicateClassStatus(ClassStatus classStatus)
    {
        string query = "SELECT COUNT(1) Count FROM ClassStatus WHERE(LOWER(Name)=LOWER(@Name) OR Id=@Id)";
        return await _db.LoadSingleAsync<bool, object>(query, classStatus);
    }
}

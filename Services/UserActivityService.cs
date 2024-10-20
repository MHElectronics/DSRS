using BOL;
using Services.Helpers;

namespace Services;
public interface IUserActivityService
{
    Task<IEnumerable<UserActivity>> GetUserActivities();
    Task<bool> InsertUserActivity(UserActivity userActivity);
}
public class UserActivityService : IUserActivityService
{
    private readonly ISqlDataAccess _db;
    public UserActivityService(ISqlDataAccess db)
    {
        _db = db;
    }
    public async Task<IEnumerable<UserActivity>> GetUserActivities() =>
        await _db.LoadData<UserActivity, dynamic>("SELECT * FROM UserActivity", new { });

    public async Task<bool> InsertUserActivity(UserActivity userActivity)
    {
        string sql = @"INSERT INTO UserActivity(UserId,DateTime,Description,Activity) VALUES(@UserId,@DateTime,@Description,@Activity)";
        return await _db.SaveData<UserActivity>(sql, userActivity);
    }
}

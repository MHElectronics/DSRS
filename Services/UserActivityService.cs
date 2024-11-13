using BOL;
using BOL.CustomModels;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Services.Helpers;

namespace Services;
public interface IUserActivityService
{
    Task<IEnumerable<UserActivity>> GetUserActivities();
    Task<IEnumerable<UserActivity>> GetUserActivitiesByFilter(UserActivity userActivity);
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

    public async Task<IEnumerable<UserActivity>> GetUserActivitiesByFilter(UserActivity userActivity)
    {
        string query = "SELECT * FROM UserActivity AS UA WHERE DATEDIFF(Day, UA.DateTime, @DateStart) <= 0 AND DATEDIFF(Day, UA.DateTime, @DateEnd) >= 0 AND UA.Activity =@Activity";
        var parameters = new
        {
            DateStart = userActivity.DateStart,
            DateEnd = userActivity.DateEnd,
            Activity = userActivity.Activity
        };
        return await _db.LoadData<UserActivity, dynamic>(query, parameters);
    }

    public async Task<bool> InsertUserActivity(UserActivity userActivity)
    {
        userActivity.DateTime = DateTime.Now;
        string sql = @"INSERT INTO UserActivity(UserId,DateTime,Description,Activity) VALUES(@UserId,@DateTime,@Description,@Activity)";
        try
        {
            var param = new
        {
            UserId = userActivity.UserId,
            DateTime = userActivity.DateTime,
            Description = userActivity.Description,
            Activity = userActivity.Activity
        };

            return await _db.SaveData<UserActivity>(sql, userActivity);
        }
        catch (Exception ex)
        {
            throw;
        }
        
    }
}

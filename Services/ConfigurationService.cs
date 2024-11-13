using BOL;
using Services.Helpers;

namespace Services;
public interface IConfigurationService
{
    Task<Configuration> GetConfiguration();
    Task<bool> InsertConfiguration(Configuration configuration, User user);
    Task<bool> UpdateConfiguration(Configuration configuration, User user);
}
public class ConfigurationService : IConfigurationService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;
    public ConfigurationService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
    }
    public async Task<Configuration> GetConfiguration() =>
        await _db.LoadSingleAsync<Configuration, dynamic>("SELECT * FROM Configuration", new { });

    public async Task<bool> InsertConfiguration(Configuration configuration, User user)
    {
        string sql = @"INSERT INTO Configuration(NumberOfAxle,SystemStartDate,WheelBaseMaximum)
            VALUES (@NumberOfAxle,@SystemStartDate,@WheelBaseMaximum)";
        bool isSuccess = await _db.SaveData<Configuration>(sql, configuration);

        if(isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "Report Configuration Inserted", LogActivity.Insert);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }

    public async Task<bool> UpdateConfiguration(Configuration configuration, User user)
    {
        string sql = @"UPDATE Configuration SET NumberOfAxle=@NumberOfAxle, SystemStartDate=@SystemStartDate, WheelBaseMaximum=@WheelBaseMaximum
                       WHERE Id=@Id";
        bool isSuccess = await _db.SaveData<Configuration>(sql, configuration);
        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "Report Configuration Updated", LogActivity.Update);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }
}

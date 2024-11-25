using BOL;
using Services.Helpers;

namespace Services;
public interface IConfigurationService
{
    Task<Configuration> GetConfiguration();
    Task<bool> InsertConfiguration(Configuration configuration, User user);
    Task<bool> UpdateConfiguration(Configuration configuration, User user);

    Task<IEnumerable<ConfigurationOverloadWeight>> GetOverloadWeights();
    Task<bool> InsertOverloadWeight(ConfigurationOverloadWeight allowedWeight, User user);
    Task<bool> UpdateOverloadWeight(ConfigurationOverloadWeight allowedWeight, User user);
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

    public async Task<IEnumerable<ConfigurationOverloadWeight>> GetOverloadWeights() =>
        await _db.LoadData<ConfigurationOverloadWeight, dynamic>("SELECT AxleNumber,AllowedWeight FROM ConfigurationOverloadWeight", new { });
    public async Task<bool> InsertOverloadWeight(ConfigurationOverloadWeight allowedWeight, User user)
    {
        string sql = @"INSERT INTO ConfigurationOverloadWeight(AxleNumber,AllowedWeight) VALUES (@AxleNumber,@AllowedWeight)";
        bool isSuccess = await _db.SaveData<ConfigurationOverloadWeight>(sql, allowedWeight);

        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "Allowed Weight Inserted", LogActivity.Insert);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }
    public async Task<bool> UpdateOverloadWeight(ConfigurationOverloadWeight allowedWeight, User user)
    {
        string sql = @"UPDATE ConfigurationOverloadWeight SET AllowedWeight=@AllowedWeight WHERE AxleNumber=@AxleNumber";
        bool isSuccess = await _db.SaveData<ConfigurationOverloadWeight>(sql, allowedWeight);
        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "Allowed Weight Updated", LogActivity.Update);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }
}

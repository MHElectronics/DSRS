using BOL;
using Services.Helpers;

namespace Services;
public interface IConfigurationService
{
    Task<Configuration> GetConfiguration();
    Task<bool> InsertConfiguration(Configuration configuration);
    Task<bool> UpdateConfiguration(Configuration configuration);
}
public class ConfigurationService : IConfigurationService
{
    private readonly ISqlDataAccess _db;
    public ConfigurationService(ISqlDataAccess db)
    {
        _db = db;
    }
    public async Task<Configuration> GetConfiguration() =>
        await _db.LoadSingleAsync<Configuration, dynamic>("SELECT * FROM Configuration", new { });

    public async Task<bool> InsertConfiguration(Configuration configuration)
    {
        string sql = @"INSERT INTO Configuration(NumberOfAxle,SystemStartDate,WheelBaseMaximum)
            VALUES (@NumberOfAxle,@SystemStartDate,@WheelBaseMaximum)";
        return await _db.SaveData<Configuration>(sql, configuration);

    }

    public async Task<bool> UpdateConfiguration(Configuration configuration)
    {
        string sql = @"UPDATE Configuration SET NumberOfAxle=@NumberOfAxle, SystemStartDate=@SystemStartDate, WheelBaseMaximum=@WheelBaseMaximum
                       WHERE Id=@Id";
        return await _db.SaveData<Configuration>(sql, configuration);
    }
}

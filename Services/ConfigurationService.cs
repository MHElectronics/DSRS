using BOL;
using Services.Helpers;

namespace Services;
public interface IConfigurationService
{
    Task<IEnumerable<Configuration>> GetConfiguration();
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
    public async Task<IEnumerable<Configuration>> GetConfiguration() =>
        await _db.LoadData<Configuration, dynamic>("SELECT * FROM Configuration", new { });

    public async Task<bool> InsertConfiguration(Configuration configuration)
    {
        string sql = @"INSERT INTO Configuration(NumberOfAxle,SystemStartDate,WheelBaseMaximum,WeightMinimum,WeightMaximum)
            VALUES (@NumberOfAxle,@SystemStartDate,@WheelBaseMaximum,@WeightMinimum,@WeightMaximum)";
        return await _db.SaveData<Configuration>(sql, configuration);

    }

    public async Task<bool> UpdateConfiguration(Configuration configuration)
    {
        string sql = @"UPDATE Configuration SET NumberOfAxle=@NumberOfAxle, SystemStartDate=@SystemStartDate, WheelBaseMaximum=@WheelBaseMaximum, WeightMinimum=@WeightMinimum, WeightMaximum=@WeightMaximum
                       WHERE Id=@Id";
        return await _db.SaveData<Configuration>(sql, configuration);
    }
}

using BOL;
using Services.Helpers;

namespace Services;
public interface IClassStatusService
{
    Task<IEnumerable<ClassStatus>> GetClassStatusList();
    Task<ClassStatus> GetClassStatus(int id);
    Task<bool> InsertClassStatus(ClassStatus classStatus);
    Task<ClassStatus> UpdateClassStatus(ClassStatus classStatus);
    Task<bool> DeleteClassStatus(int id);
}
public class ClassStatusService : IClassStatusService
{
    private readonly ISqlDataAccess _db;
    public ClassStatusService(ISqlDataAccess db)
    {
        _db = db;
    }

    public async Task<bool> DeleteClassStatus(int id)
    {
        string query = "DELETE FROM ClassStatus WHERE Id=@Id";
        int count = await _db.DeleteData<ClassStatus, object>(query, new { id });
        return count > 0;
    }

    public async Task<ClassStatus> GetClassStatus(int id)
    {
        var results = await _db.LoadData<ClassStatus, dynamic>("SELECT * FROM ClassStatus WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<ClassStatus>> GetClassStatusList() =>
        await _db.LoadData<ClassStatus, dynamic>("SELECT * FROM ClassStatus", new { });

    public async Task<bool> InsertClassStatus(ClassStatus classStatus)
    {
        string sql = @"INSERT INTO ClassStatus(Name) VALUES(@Name)";
        return await _db.SaveData<ClassStatus>(sql, classStatus);
    }

    public async Task<ClassStatus> UpdateClassStatus(ClassStatus classStatus)
    {
        string sql = @"UPDATE ClassStatus SET Name=@Name WHERE Id=@Id";
        await _db.SaveData(sql, classStatus);
        return classStatus;
    }
}

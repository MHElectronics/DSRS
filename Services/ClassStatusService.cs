using BOL;
using Services.Helpers;

namespace Services;
public interface IClassStatusService
{
    Task<IEnumerable<ClassStatus>> GetClassStatusList();
    Task<ClassStatus> GetClassStatus(int id);
    Task<ClassStatus> InsertClassStatus(ClassStatus classStatus);
    Task<ClassStatus> UpdateClassStatus(ClassStatus classStatus);
    Task<ClassStatus> DeleteClassStatus(int id);
}
public class ClassStatusService : IClassStatusService
{
    private readonly ISqlDataAccess _db;
    public ClassStatusService(ISqlDataAccess db)
    {
        _db = db;
    }

    public async Task<ClassStatus> DeleteClassStatus(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<ClassStatus> GetClassStatus(int id)
    {
        var results = await _db.LoadData<ClassStatus, dynamic>("SELECT * FROM ClassStatus WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<ClassStatus>> GetClassStatusList() =>
        await _db.LoadData<ClassStatus, dynamic>("SELECT * FROM ClassStatus", new { });

    public async Task<ClassStatus> InsertClassStatus(ClassStatus classStatus)
    {
        int id = await _db.Insert<ClassStatus>(classStatus);

        if (id != 0)
        {
            classStatus.Id = id;
            return classStatus;
        }

        return null;
    }

    public async Task<ClassStatus> UpdateClassStatus(ClassStatus classStatus)
    {
        string sql = @"UPDATE ClassStatus SET Name=@Name WHERE Id=@Id";
        await _db.SaveData(sql, classStatus);
        return classStatus;
    }
}

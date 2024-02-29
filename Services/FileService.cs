using BOL;
using Services.Helpers;

namespace Services;
public interface IFileService
{
    Task<IEnumerable<Files>> Get(int stationId = 0, DateTime? date = null);
    Task<Files> GetById(Files file);
    Task<Files> Upload(byte[] fileBytes, Files file);
    Task<bool> Update();
    Task<bool> Delete();
}
public class FileService : IFileService
{
    private readonly ISqlDataAccess _db;
    public FileService(ISqlDataAccess db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Files>> Get(int stationId = 0, DateTime? date = null)
    {
        string sql = "SELECT Id,StationId,Date,FileType,FileName,ManualUpload,UploadDate,IsProcessed FROM Files WHERE 1=1 ";
        Dictionary<string, object> param = new Dictionary<string, object>();

        if (stationId > 0)
        {
            param.Add("@StationId", stationId);
        }
        if(date != null)
        {
            param.Add("@Date", date);
        }

        return await _db.LoadData<Files, dynamic>("SELECT * FROM Files", param);
    }


    public async Task<Files> GetById(Files file)
    {
        string sql = "SELECT Id,StationId,Date,FileType,FileName,ManualUpload,UploadDate,IsProcessed FROM Files WHERE Id=@Id";
        return await _db.LoadSingleAsync<Files, dynamic>(sql, file.Id);
    }

    public Task<Files> Upload(byte[] fileBytes, Files file)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Update()
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete()
    {
        throw new NotImplementedException();
    }
}

using BOL;
using Services.Helpers;
using System.Data;

namespace Services;
public interface IFileService
{
    Task<IEnumerable<Files>> Get(int stationId = 0, DateTime? date = null);
    Task<Files> GetById(Files file);
    Task<bool> Add(Files obj);
    Task<Files> Upload(byte[] fileBytes, Files file);
    Task<bool> Update(Files obj);
}
public class FileService : IFileService
{
    private readonly ISqlDataAccess _db;
    private readonly IFtpHelper _ftpHelper;
    public FileService(ISqlDataAccess db, IFtpHelper ftpHelper)
    {
        _db = db;
        _ftpHelper = ftpHelper;
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

        return await _db.LoadData<Files, dynamic>(sql, param);
    }

    public async Task<Files> GetById(Files file)
    {
        string sql = "SELECT Id,StationId,Date,FileType,FileName,ManualUpload,UploadDate,IsProcessed FROM Files WHERE Id=@Id";
        return await _db.LoadSingleAsync<Files, dynamic>(sql, file.Id);
    }

    public async Task<Files> Upload(byte[] fileBytes, Files file)
    {
        string destinationTableName = "AxleLoadMeasuredData";
        bool FileUploaded = await _ftpHelper.UploadFile(fileBytes, file.FileName, "");
        if (FileUploaded)
        {
            if(await this.Add(file))
            {
                try
                {
                    DataTable csvData = await _ftpHelper.GetDataTabletFromCSVFile(file.FileName);
                    if (csvData is not null)
                    {
                        await _db.InsertDataTable(csvData, destinationTableName);

                        file.IsProcessed = true;
                        bool isUpdated = await this.Update(file);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        return file;
    }

    public Task<bool> Update(Files obj)
    {
        string query = @"UPDATE Files
            SET StationId=@StationId
            ,Date=@Date
            ,FileType=@FileType
            ,FileName=@FileName
            ,ManualUpload=@ManualUpload
            ,UploadDate=@UploadDate
            ,IsProcessed=@IsProcessed
            WHERE Id=@Id";
        return _db.SaveData(query, obj);
    }

    public async Task<bool> Add(Files obj)
    {
        obj.Id = await _db.Insert<Files>(obj);
        return obj.Id > 0;
    }
}

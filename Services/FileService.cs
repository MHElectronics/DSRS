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
    private readonly ICsvHelper _csvHelper;
    public FileService(ISqlDataAccess db, IFtpHelper ftpHelper, ICsvHelper csvHelper)
    {
        _db = db;
        _ftpHelper = ftpHelper;
        _csvHelper = csvHelper;
    }

    public async Task<IEnumerable<Files>> Get(int stationId = 0, DateTime? date = null)
    {
        string sql = "SELECT Id,StationId,Date,FileType,FileName,ManualUpload,UploadDate,IsProcessed FROM Files WHERE 1=1 ";
        Dictionary<string, object> param = new Dictionary<string, object>();

        if (stationId > 0)
        {
            param.Add("@StationId", stationId);
        }
        if (date != null)
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
        //Generate file name
        file.FileName = this.GetFileName(file);
        //Set upload date time
        file.UploadDate = DateTime.Now;
        //Set File Date time
        file.Date = File.GetCreationTime(file.FileName);
        bool FileUploaded = await _ftpHelper.UploadFile(fileBytes, file.FileName, "");

        if (FileUploaded)
        {
            if (await this.Add(file))
            {
                try
                {
                    //DataTable csvData = await _ftpHelper.GetDataTableFromCSV(file.FileName);
                    DataTable csvData = _csvHelper.GetDataTableFromByte(fileBytes);
                    if (csvData is not null)
                    {
                        await _db.InsertDataTable(csvData, destinationTableName: "AxleLoadMeasuredData");

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
    private string GetFileName(Files file)
    {
        return "S" + file.StationId + "_" + file.Date.ToString("yyyyMMdd") + Path.GetExtension(file.FileName);
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
        string query = "INSERT INTO Files(StationId,Date,FileType,FileName,ManualUpload,UploadDate,IsProcessed) VALUES(@StationId,@Date,@FileType,@FileName,@ManualUpload,@UploadDate,@IsProcessed)";
        return await _db.SaveData<Files>(query, obj);
    }
}

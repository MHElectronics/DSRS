using BOL;
using Services.Helpers;
using System.Data;

namespace Services;
public interface IFileService
{
    Task<IEnumerable<UploadedFile>> Get(int stationId = 0, DateTime? date = null);
    Task<UploadedFile> GetById(UploadedFile file);
    Task<bool> Add(UploadedFile obj);
    Task<UploadedFile> Upload(byte[] fileBytes, UploadedFile file);
    Task<bool> Update(UploadedFile obj);
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

    public async Task<IEnumerable<UploadedFile>> Get(int stationId = 0, DateTime? date = null)
    {
        string sql = "SELECT Id,StationId,Date,FileType,FileName,ManualUpload,UploadDate,IsProcessed FROM UploadedFile WHERE 1=1 ";
        Dictionary<string, object> param = new Dictionary<string, object>();

        if (stationId > 0)
        {
            param.Add("@StationId", stationId);
        }
        if (date != null)
        {
            param.Add("@Date", date);
        }

        return await _db.LoadData<UploadedFile, dynamic>(sql, param);
    }

    public async Task<UploadedFile> GetById(UploadedFile file)
    {
        string sql = "SELECT Id,StationId,Date,FileType,FileName,ManualUpload,UploadDate,IsProcessed FROM UploadedFile WHERE Id=@Id";
        return await _db.LoadSingleAsync<UploadedFile, dynamic>(sql, file.Id);
    }

    public async Task<UploadedFile> Upload(byte[] fileBytes, UploadedFile file)
    {
        //Generate file name
        file.FileName = this.GetFileName(file);
        //Set upload date time
        file.UploadDate = DateTime.Now;

        bool FileUploaded = await _ftpHelper.UploadFile(fileBytes, file.FileName, "");

        if (FileUploaded)
        {
            if (await this.Add(file))
            {
                try
                {
                    //DataTable csvData = await _ftpHelper.GetDataTableFromCSV(file.FileName);
                    DataTable csvData = _csvHelper.GetDataTableFromByte(fileBytes, file);
                    if (csvData is not null)
                    {
                        string destinationTableName = "AxleLoadProcess";
                        if (file.FileType == (int)UploadedFileType.FineData)
                        {
                            destinationTableName = "FinePaymentProcess";
                        }

                        await _db.InsertDataTable(csvData, destinationTableName);
                        
                        //Process inserted data
                        //await this.RunProcess(file);
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
    private async Task<bool> RunProcess(UploadedFile file)
    {
        string query = "EXEC dbo.ProcessAxleLoad FileId=@Id";
        if (file.FileType == (int)UploadedFileType.FineData)
        {
            query = "EXEC dbo.ProcessFinePayment FileId=@Id";
        }

        return await _db.SaveData(query, new { file.Id });
    }
    private string GetFileName(UploadedFile file)
    {
        return "S" + file.StationId + "_" + file.Date.ToString("yyyyMMdd") + "_" + file.FileType.ToString() + ".csv";
    }
    public Task<bool> Update(UploadedFile obj)
    {
        string query = @"UPDATE UploadedFiles
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

    public async Task<bool> Add(UploadedFile obj)
    {
        obj.Id = await _db.Insert<UploadedFile>(obj);
        return obj.Id > 0;
    }
}

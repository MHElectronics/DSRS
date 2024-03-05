using BOL;
using Services.Helpers;
using System.Data;

namespace Services;
public interface IFileService
{
    Task<IEnumerable<Files>> Get(int stationId = 0, DateTime? date = null);
    Task<Files> GetById(Files file);
    Task<bool> Add(LoadDataSlowMoving obj);
    Task<bool> Add(List<LoadDataSlowMoving> obj);
    Task<bool> Add(LoadDataFastMoving obj);
    Task<bool> Add(List<LoadDataFastMoving> obj);
    Task<Files> Upload(byte[] fileBytes, Files file);
    Task<bool> Update();
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

        return await _db.LoadData<Files, dynamic>("SELECT * FROM Files", param);
    }


    public async Task<Files> GetById(Files file)
    {
        string sql = "SELECT Id,StationId,Date,FileType,FileName,ManualUpload,UploadDate,IsProcessed FROM Files WHERE Id=@Id";
        return await _db.LoadSingleAsync<Files, dynamic>(sql, file.Id);
    }

    public async Task<Files> Upload(byte[] fileBytes, Files file)
    {
        string destinationTableName = "FinePayment";
        bool test = await _ftpHelper.UploadFile(fileBytes,file.FileName, "AxleLoad");
        if (test)
        {
            DataTable csvData = await _ftpHelper.GetDataTabletFromCSVFile(file.FileName);
            await _db.InsertDataTable(csvData, destinationTableName);

        }
        return file;
    }

    public Task<bool> Update()
    {
        throw new NotImplementedException();
    }


    public Task<bool> Add(LoadDataSlowMoving obj)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Add(List<LoadDataSlowMoving> obj)
    {
        throw new NotImplementedException();
    }
    public Task<bool> Add(LoadDataFastMoving obj)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Add(List<LoadDataFastMoving> obj)
    {
        throw new NotImplementedException();
    }
}

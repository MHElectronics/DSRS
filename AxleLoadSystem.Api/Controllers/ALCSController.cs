using AxleLoadSystem.Api.Extensions;
using AxleLoadSystem.Api.Models;
using BOL;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace AxleLoadSystem.Api.Controllers;

[CustomAuthorize]
[ApiController]
[Route("[controller]")]
public class ALCSController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IAxleLoadService _axleLoadService;
    private readonly IFinePaymentService _finePaymentService;
    private readonly IWIMScaleService _wimScaleService;
    private (bool, string) isSuccess;

    public ALCSController(IFileService fileService, IAxleLoadService axleLoadService, IFinePaymentService finePaymentService, IWIMScaleService wimScaleService)
    {
        _fileService = fileService;
        _axleLoadService = axleLoadService;
        _finePaymentService = finePaymentService;
        _wimScaleService = wimScaleService;
    }

    [HttpGet("[action]")]
    public IActionResult Check()
    {
        return Ok("Connected");
    }

    [HttpGet("[action]/{date}")]
    public async Task<IActionResult> LoadDataFileExists(DateTime date)
    {
        UploadedFile file = new()
        {
            StationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString()),
            Date = date,
            FileType = (int)UploadedFileType.LoadData
        };

        bool exists = await _fileService.FileExists(file);
        
        return Ok(exists);
    }
    [HttpGet("[action]/{date}")]
    public async Task<IActionResult> FineDataFileExists(DateTime date)
    {
        UploadedFile file = new()
        {
            StationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString()),
            Date = date,
            FileType = (int)UploadedFileType.FineData
        };

        bool exists = await _fileService.FileExists(file);

        return Ok(exists);
    }

    #region CSV File Upload
    [DisableRequestSizeLimit]
    //[ServiceFilter(typeof(ModelValidationAttribute))]
    [HttpPost("[action]")]
    public async Task<IActionResult> UploadLoadData([ModelBinder(BinderType = typeof(JsonModelBinder))] FileUploadModel station, IFormFile uploadFile)
    {
        if (uploadFile == null || uploadFile.Length == 0 || station == null)
        {
            return new BadRequestResult();
        }

        if (Path.GetExtension(uploadFile.FileName).ToLower() != ".csv")
        {
            return BadRequest("Only CSV files are allowed.");
        }

        // Check station code
        string stationId = this.HttpContext.Request.Headers["StationId"].ToString();
        if (Convert.ToInt16(stationId) != station.StationId)
        {
            return BadRequest("Station Id doesn't match");
        }

        if (station.Date >= DateTime.Today)
        {
            return BadRequest("Only date before today is allowed");
        }

        station.StationId = Convert.ToInt16(stationId);
        UploadedFile file = station.ToUploadedFile();
        file.FileType = (int)UploadedFileType.LoadData;

        try
        {
            using (var stream = uploadFile.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                string headerLine = await reader.ReadLineAsync();
                reader.Close();
                reader.Dispose();   
                stream.Close();
                stream.Dispose();
                if (string.IsNullOrEmpty(headerLine))
                {
                    return BadRequest("CSV file is empty or missing headers.");
                }

                headerLine = headerLine.Replace(" ", "");

                string requiredHeaders = "TransactionNumber,LaneNumber,DateTime,PlateZone,PlateSeries,PlateNumber,VehicleId," +
                    "NumberOfAxle,VehicleSpeed,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7,AxleRemaining,GrossVehicleWeight,IsUnloaded,IsOverloaded," +
                    "OverSizedModified,Wheelbase,ClassStatus,RecognizedBy,IsBRTAInclude,LadenWeight,UnladenWeight,ReceiptNumber,BillNumber,Axle1Time," +
                    "Axle2Time,Axle3Time,Axle4Time,Axle5Time,Axle6Time,Axle7Time";
                if (requiredHeaders.ToLower() != headerLine.ToLower())
                {
                    return BadRequest("Wrong Header");
                }
            }
            if (await _fileService.FileExists(file))
            {
                return BadRequest("File already uploaded");
            }
            file = await this.UploadFile(file, uploadFile);
            if (file.Id > 0)
            {
                return Ok("Axle load file uploaded successfully");
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return BadRequest("Error: Axle load file upload failed");
    }

    [CustomAuthorize]
    [DisableRequestSizeLimit]
    //[ServiceFilter(typeof(ModelValidationAttribute))]
    [HttpPost("[action]")]
    public async Task<IActionResult> UploadFineData([ModelBinder(BinderType = typeof(JsonModelBinder))] FileUploadModel station, IFormFile uploadFile)
    {
        if (uploadFile == null || uploadFile.Length == 0 || station == null)
        {
            return new BadRequestResult();
        }
        if (Path.GetExtension(uploadFile.FileName).ToLower() != ".csv")
        {
            return BadRequest("Only CSV files are allowed.");
        }
        //Check station code
        string stationId = this.HttpContext.Request.Headers["StationId"].ToString();
        //string apiKey = this.HttpContext.Response.Headers["ApiKey"].ToString();
        //string key = this.HttpContext.Response.Headers.Authorization[0].ToString();

        if (Convert.ToInt16(stationId) != station.StationId)
        {
            return BadRequest("Station Id doesn't match");
        }
        if (station.Date >= DateTime.Today)
        {
            return BadRequest("Only date before today is allowed");
        }

        station.StationId = Convert.ToInt16(stationId);
        UploadedFile file = station.ToUploadedFile();
        file.FileType = (int)UploadedFileType.FineData;

        try
        {
            using (var stream = uploadFile.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                string headerLine = await reader.ReadLineAsync();
                reader.Close();
                reader.Dispose();
                stream.Close();
                stream.Dispose();
                if (string.IsNullOrEmpty(headerLine))
                {
                    return BadRequest("CSV file is empty or missing headers");
                }

                headerLine = headerLine.Replace(" ", "");

                string requiredHeaders = "LaneNumber,TransactionNumber,PaymentTransactionId,DateTime,IsPaid,FineAmount,PaymentMethod,ReceiptNumber," +
                    "BillNumber,WarehouseCharge,DriversLicenseNumber,TransportAgencyInformation";
                if (requiredHeaders.ToLower() != headerLine.ToLower())
                {
                    return BadRequest("Wrong Header");
                }
            }

            if (await _fileService.FileExists(file))
            {
                return BadRequest("File already uploaded");
            }
            file = await this.UploadFile(file, uploadFile);
            if (file.Id > 0)
            {
                return Ok("Fine payment file uploaded successfully");
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return BadRequest("Error: Fine payment file upload failed");
    }
    private async Task<UploadedFile> UploadFile(UploadedFile file, IFormFile uploadFile)
    {
        byte[] byteFile;
        using (MemoryStream ms = new MemoryStream())
        {
            uploadFile.CopyTo(ms);
            byteFile = ms.ToArray();
        }

        file.ManualUpload = false;

        return await _fileService.Upload(byteFile, file);
    }
    #endregion
    
    [HttpPost("[action]")]
    public async Task<IActionResult> LoadData(LoadData obj)
    {
        if (obj == null)
        {
            return new BadRequestResult();
        }
        // Single Datapoint entry is allowed only today's and yesterday's Data
        if (obj.DateTime.Date != DateTime.Today && obj.DateTime.Date != DateTime.Today.AddDays(-1))
        {
            return BadRequest("Only today's and yesterday's data is allowed");
        }

        //Get station id from authentication
        obj.StationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());

        List<LoadData> validData = await this.CheckValidData(obj.StationId, new List<LoadData> { obj });
        if (validData.Count > 0)
        {
            isSuccess = await _axleLoadService.Add(validData);
            if(isSuccess.Item1)
            {
                return Ok("Axle load data insert successful");
            }
            return BadRequest(isSuccess.Item2);
        }

        return BadRequest("Error: Axle load data validation failed");
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> LoadDataMultiple(List<LoadData> obj)
    {
        if (obj == null)
        {
            return new BadRequestResult();
        }
        // Multiple Datapoint entry is allowed only today's and yesterday's Data
        if (obj.Any(l => l.DateTime.Date != DateTime.Today && l.DateTime.Date != DateTime.Today.AddDays(-1)))
        {
            return BadRequest("Only today's and yesterday's data is allowed");
        }

        //Check station code
        int stationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());

        List<LoadData> validData = await this.CheckValidData(stationId, obj);
        
        if (validData.Count > 0)
        {
            foreach (var item in validData)
            {
                item.StationId = stationId;
            }

            isSuccess = await _axleLoadService.Add(validData);
            if (isSuccess.Item1)
            {
                return Ok("Axle load multiple data insert successful");
            }
            return BadRequest(isSuccess.Item2);
        }

        return BadRequest("Error: Axle load multiple data validation failed");
    }
    private async Task<List<LoadData>> CheckValidData(int stationId, List<LoadData> data)
    {
        //Data check
        data.RemoveAll(d => d.DateTime.Date != DateTime.Today && d.DateTime.Date != DateTime.Today.AddDays(-1));

        //Check lane number
        IEnumerable<WIMScale> wims = await _wimScaleService.GetByStation(new WIMScale(){ StationId = stationId });
        data.RemoveAll(d => !wims.Any(w => w.LaneNumber == d.LaneNumber));

        return data;
    }
    private async Task<List<FinePayment>> CheckValidFineData(int stationId, List<FinePayment> data)
    {
        //Data check
        data.RemoveAll(d => d.DateTime.Date != DateTime.Today && d.DateTime.Date != DateTime.Today.AddDays(-1));

        //Check lane number
        IEnumerable<WIMScale> wims = await _wimScaleService.GetByStation(new WIMScale() { StationId = stationId });
        data.RemoveAll(d => !wims.Any(w => w.LaneNumber == d.LaneNumber));

        return data;
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> FinePayment(FinePayment obj)
    {
        if (obj == null)
        {
            return new BadRequestResult();
        }
        // Single Datapoint entry is allowed only today's and yesterday's Data
        if (obj.DateTime.Date != DateTime.Today && obj.DateTime.Date != DateTime.Today.AddDays(-1))
        {
            return BadRequest("Only today's and yesterday's data is allowed");
        }

        //Check station code
        obj.StationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());
        List<FinePayment> validData = await this.CheckValidFineData(obj.StationId, new List<FinePayment> { obj });
        if (validData.Count > 0)
        {
            isSuccess = await _finePaymentService.Add(obj);
            if (isSuccess.Item1)
            {
                return Ok("Fine payment data insert successful");
            }
            return BadRequest(isSuccess.Item2);
        }

        return BadRequest("Error: Fine payment validation failed");
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> FinePaymentMultiple(List<FinePayment> obj)
    {
        if (obj == null)
        {
            return new BadRequestResult();
        }
        // Multiple Datapoint entry is allowed only today's and yesterday's Data
        if (obj.Any(l => l.DateTime.Date != DateTime.Today && l.DateTime.Date != DateTime.Today.AddDays(-1)))
        {
            return BadRequest("Only today's and yesterday's data is allowed");
        }

        //Check station code
        int stationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());

        foreach(var item in obj)
        {
            item.StationId = stationId;
        }

        List<FinePayment> validData = await this.CheckValidFineData(stationId,  obj );
        if (validData.Count > 0)
        {
            isSuccess = await _finePaymentService.Add(obj);
            if (isSuccess.Item1)
            {
                return Ok("Fine payment multiple data insert successful");
            }
            return BadRequest(isSuccess.Item2);
        }
        return BadRequest("Error: Fine payment multiple validation failed");
    }
}

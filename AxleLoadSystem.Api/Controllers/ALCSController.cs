using AxleLoadSystem.Api.Extensions;
using AxleLoadSystem.Api.Models;
using BOL;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    private readonly ILogger<ALCSController> _logger;
    private (bool, string) isSuccess;

    public ALCSController(IFileService fileService, IAxleLoadService axleLoadService, IFinePaymentService finePaymentService, IWIMScaleService wimScaleService, ILogger<ALCSController> logger)
    {
        _fileService = fileService;
        _axleLoadService = axleLoadService;
        _finePaymentService = finePaymentService;
        _wimScaleService = wimScaleService;
        _logger = logger;
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

        try
        {
            bool exists = await _fileService.FileExists(file);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadDataFileExists");
            return BadRequest("Error occured");
        }
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

        try
        {
            bool exists = await _fileService.FileExists(file);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadDataFileExists");
            return BadRequest("Error occured");
        }
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
            _logger.LogError(ex, "UploadLoadData");
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
            _logger.LogError(ex, "UploadFineData");
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

        (List<LoadData> Data, string Message) validData = await this.CheckValidData(obj.StationId, new List<LoadData> { obj });

        if (validData.Data.Count > 0)
        {
            foreach (var loadData in validData.Data)
            {
                loadData.StationId = obj.StationId;
            }

            var isSuccess = await _axleLoadService.Add(validData.Data);

            if (isSuccess.Item1)
            {
                return Ok("Axle load multiple data insert successful" + "|" + validData.Message);
            }

            _logger.LogError("Station " + obj.StationId + ": " + isSuccess.Item2);
            return BadRequest(isSuccess.Item2 + "|" + validData.Message);
        }
        else
        {
            return BadRequest("No valid data to insert." + "|" + validData.Message);
        }
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

        // Lane Number validation
        IEnumerable<WIMScale> wims = await _wimScaleService.GetByStation(new WIMScale() { StationId = stationId });
        if (obj.Any(d => !wims.Any(w => w.LaneNumber == d.LaneNumber)))
        {
            return BadRequest("Wrong Lane Number Count: " + obj.Count(d => !wims.Any(w => w.LaneNumber == d.LaneNumber)));
        }

        (List<LoadData> Data, string Message) validData = await this.CheckValidData(stationId, obj);

        if (validData.Data.Count > 0)
        {
            foreach (var loadData in validData.Data)
            {
                loadData.StationId = stationId; 
            }

            var isSuccess = await _axleLoadService.Add(validData.Data);

            if (isSuccess.Item1)
            {
                return Ok("Axle load multiple data inserted successfully"+ "|" + validData.Message);
            }

            _logger.LogError("Station " + stationId + ": " + isSuccess.Item2);
            return BadRequest(isSuccess.Item2 + "|" + validData.Message);
        }
        else
        {
            return BadRequest("No valid data to insert." + "|" + validData.Message);
        }
    }
    private async Task<(List<LoadData> ValidData, string Message)> CheckValidData(int stationId, List<LoadData> data)
    {
        string message = "";
        //Data check
        data.RemoveAll(d => d.DateTime.Date != DateTime.Today && d.DateTime.Date != DateTime.Today.AddDays(-1));

        //Check lane number
        IEnumerable<WIMScale> wims = await _wimScaleService.GetByStation(new WIMScale() { StationId = stationId });
        if (data.Any(d => !wims.Any(w => w.LaneNumber == d.LaneNumber)))
        {
            _logger.LogInformation("Wrong Lane Number. station:" + stationId + ". Count:" + data.Count(d => !wims.Any(w => w.LaneNumber == d.LaneNumber)));
            message += "Wrong Lane Number Count: " + data.Count(d => !wims.Any(w => w.LaneNumber == d.LaneNumber)) + "|";
            data.RemoveAll(d => !wims.Any(w => w.LaneNumber == d.LaneNumber));
        }  

        //Check Number of Axle
        if (data.Any(d => d.NumberOfAxle < 2))
        {
            _logger.LogInformation("Number of axle less then 2. station: " + stationId + ". Count: " + data.Count(d => d.NumberOfAxle < 2));
            message += " Number of axles must be 2 or higher. Count: " + data.Count(d => d.NumberOfAxle < 2) + "|";
            data.RemoveAll(d => d.NumberOfAxle < 2);
        }
        
        //Check Load Data value zero for Axle 1 to 7
        if (data.Any(l => l.Axle1 == 0 && l.Axle2 == 0 && l.Axle3 == 0 &&
                l.Axle4 == 0 && l.Axle5 == 0 && l.Axle6 == 0 && l.Axle7 == 0))
        {
            _logger.LogInformation("Axle 1 to Axle 7 must be provided for station " + stationId + ". Count: " + data.Count(l => l.Axle1 == 0 && l.Axle2 == 0 && l.Axle3 == 0 &&
                l.Axle4 == 0 && l.Axle5 == 0 && l.Axle6 == 0 && l.Axle7 == 0));
            message += " Axle 1 to Axle 7  must be provided. Count: " + data.Count(l => l.Axle1 == 0 && l.Axle2 == 0 && l.Axle3 == 0 &&
                l.Axle4 == 0 && l.Axle5 == 0 && l.Axle6 == 0 && l.Axle7 == 0) + "|";
            data.RemoveAll(l => l.Axle1 == 0 && l.Axle2 == 0 && l.Axle3 == 0 &&
               l.Axle4 == 0 && l.Axle5 == 0 && l.Axle6 == 0 && l.Axle7 == 0);
        }
       
        //Check Load Data value zero for Axle Remaining
        if (data.Any(l => l.NumberOfAxle >= 8 && l.AxleRemaining == 0))
        {
            _logger.LogInformation("Axle Remaining must be provided for station " + stationId + ". Count: " + data.Count(l => l.NumberOfAxle >= 8 && l.AxleRemaining == 0));
            message += " Axle Remaining must be provided. Count: " + data.Count(l => l.NumberOfAxle >= 8 && l.AxleRemaining == 0) + "|";
            data.RemoveAll(l => l.NumberOfAxle >= 8 && l.AxleRemaining == 0);
        }

        return (data, message);
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

            _logger.LogError("FinePayment - Station " + obj.StationId + ": " + isSuccess.Item2);
            return BadRequest(isSuccess.Item2);
        }

        _logger.LogError("Error: Fine payment validation failed");
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

        foreach (var item in obj)
        {
            item.StationId = stationId;
        }

        List<FinePayment> validData = await this.CheckValidFineData(stationId, obj);
        if (validData.Count > 0)
        {
            isSuccess = await _finePaymentService.Add(obj);
            if (isSuccess.Item1)
            {
                return Ok("Fine payment multiple data insert successful");
            }

            _logger.LogError("FinePayment - Station " + stationId + ": " + isSuccess.Item2);
            return BadRequest(isSuccess.Item2);
        }

        _logger.LogError("Error: Fine payment multiple validation failed");
        return BadRequest("Error: Fine payment multiple validation failed");
    }
}

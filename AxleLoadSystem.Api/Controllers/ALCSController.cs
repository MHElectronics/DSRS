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
        
        //Get station id from authentication
        obj.StationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());

        (List<LoadData> Data, string Message) validData = await this.CheckValidData(obj.StationId, new List<LoadData> { obj }, false);

        if (validData.Data.Count > 0)
        {
            foreach (var loadData in validData.Data)
            {
                loadData.StationId = obj.StationId;
            }

            var isSuccess = await _axleLoadService.Add(validData.Data);

            if (isSuccess.Item1)
            {
                return Ok("Axle load data insert successful");
            }

            _logger.LogError("Axle Load single db insert failed - Station " + obj.StationId + ": " + isSuccess.Item2);
            return BadRequest(isSuccess.Item2 + "|" + validData.Message);
        }

        return BadRequest("No valid data found." + "|" + validData.Message);
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> LoadDataMultiple(List<LoadData> obj)
    {
        if (obj == null)
        {
            return new BadRequestResult();
        }
        
        //Check station id
        int stationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());

        (List<LoadData> Data, string Message) validData = await this.CheckValidData(stationId, obj, true);

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

            _logger.LogError("Axle Load multiple db insert failed - Station " + stationId + ": " + isSuccess.Item2);
            return BadRequest(isSuccess.Item2 + "|" + validData.Message);
        }
        
        return BadRequest("No valid data found." + "|" + validData.Message);
    }
    private async Task<(List<LoadData> ValidData, string Message)> CheckValidData(int stationId, List<LoadData> data, bool isMultiple)
    {
        string message = "";
        int count = 0;

        //Data check
        count = data.RemoveAll(d => d.DateTime.Date != DateTime.Today && d.DateTime.Date != DateTime.Today.AddDays(-1));
        if (count > 0)
        {
            message += "Wrong Date:" + count + "|";
            count = 0;
        }

        //Check lane number
        IEnumerable<WIMScale> wims = await _wimScaleService.GetByStation(new WIMScale() { StationId = stationId });
        count = data.RemoveAll(d => !wims.Any(w => w.LaneNumber == d.LaneNumber));
        if (count > 0)
        {
            message += "Wrong Lane Number: " + count + "|";
            count = 0;
        }

        //Check Number of Axle
        count = data.RemoveAll(d => d.NumberOfAxle < 2);
        if (count > 0)
        {
            message += " Number of axles must be 2 or higher: " + count + "|";
            count = 0;
        }

        //Check Load Data value zero for Axle 1 to 7
        count = data.RemoveAll(l => l.Axle1 == 0 && l.Axle2 == 0 && l.Axle3 == 0 && l.Axle4 == 0 && l.Axle5 == 0 && l.Axle6 == 0 && l.Axle7 == 0);
        if (count > 0)
        {
            message += " Axle 1 to 7  must be provided: " + count + "|";
            count = 0;
        }
       
        //Check Load Data value zero for Axle Remaining
        count = data.RemoveAll(l => l.NumberOfAxle >= 8 && l.AxleRemaining == 0);
        if (count > 0)
        {
            message += " Axle Remaining must be provided: " + count + "|";
            count = 0;
        }

        if (!string.IsNullOrEmpty(message))
        {
            _logger.LogInformation("Station: " + stationId + ". Invalid load data " + (isMultiple ? "multiple:" : "single:") + message);
        }

        return (data, message);
    }
    
    [HttpPost("[action]")]
    public async Task<IActionResult> FinePayment(FinePayment obj)
    {
        if (obj == null)
        {
            return new BadRequestResult();
        }
        
        //Check station id
        obj.StationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());
        (List<FinePayment> Data, string Message) validData = await this.CheckValidFineData(obj.StationId, new List<FinePayment> { obj }, false);
        if (validData.Data.Count > 0)
        {
            isSuccess = await _finePaymentService.Add(validData.Data);
            if (isSuccess.Item1)
            {
                return Ok("Fine payment data insert successful");
            }

            _logger.LogError("FinePayment single db insert failed - Station " + obj.StationId + ": " + isSuccess.Item2);
            return BadRequest(isSuccess.Item2);
        }

        _logger.LogError("FinePayment single validation failed - Station " + obj.StationId);
        return BadRequest("No valid data found." + "|" + validData.Message);
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> FinePaymentMultiple(List<FinePayment> obj)
    {
        if (obj == null)
        {
            return new BadRequestResult();
        }
        
        //Check station code
        int stationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());

        (List<FinePayment> Data, string Message) validData = await this.CheckValidFineData(stationId, obj, true);
        if (validData.Data.Count > 0)
        {
            foreach (var item in validData.Data)
            {
                item.StationId = stationId;
            }

            isSuccess = await _finePaymentService.Add(validData.Data);
            if (isSuccess.Item1)
            {
                return Ok("Fine payment multiple data insert successful");
            }

            _logger.LogError("FinePayment multiple db insert failed - Station " + stationId + ": " + isSuccess.Item2);
            return BadRequest(isSuccess.Item2 + "|" + validData.Message);
        }

        return BadRequest("No valid data found." + "|" + validData.Message);
    }
    private async Task<(List<FinePayment>, string Message)> CheckValidFineData(int stationId, List<FinePayment> data, bool isMultiple)
    {
        int count = 0;
        string message = string.Empty;

        //Data check
        count = data.RemoveAll(d => d.DateTime.Date != DateTime.Today && d.DateTime.Date != DateTime.Today.AddDays(-1));
        if (count > 0)
        {
            message = "Wrong Date:" + count + "|";
            count = 0;
        }

        //Check lane number
        IEnumerable<WIMScale> wims = await _wimScaleService.GetByStation(new WIMScale() { StationId = stationId });
        count = data.RemoveAll(d => !wims.Any(w => w.LaneNumber == d.LaneNumber));
        if (count > 0)
        {
            message += "Wrong Lane Number:" + count + "|";
            count = 0;
        }

        if (!string.IsNullOrEmpty(message))
        {
            _logger.LogInformation("Station: " + stationId + ". Invalid fine data " + (isMultiple ? "multiple:" : "single:") + message);
        }

        return (data, message);
    }
}

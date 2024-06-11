using AxleLoadSystem.Api.Extensions;
using AxleLoadSystem.Api.Models;
using BOL;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace AxleLoadSystem.Api.Controllers
{
	[CustomAuthorize]
    [ApiController]
    [Route("[controller]")]
    public class ALCSController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IAxleLoadService _axleLoadService;
        private readonly IFinePaymentService _finePaymentService;
        private readonly IWIMScaleService _wimScaleService;

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

        [HttpGet("[action]")]
        public async Task<IActionResult> LoadDataFileExists(int stationId, DateTime date)
        {
            UploadedFile file = new()
            {
                StationId = stationId,
                Date = date,
                FileType = (int)UploadedFileType.LoadData
            };

            bool exists = await _fileService.FileExists(file);
            
            return Ok(exists);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> FineDataFileExists(int stationId, DateTime date)
        {
            UploadedFile file = new()
            {
                StationId = stationId,
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
            file.FileType = (int)UploadedFileType.LoadData;
            
            if (await _fileService.FileExists(file))
            {
                return BadRequest("File already uploaded");
            }

            try
            {
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
            if(station.Date >= DateTime.Today)
            {
                return BadRequest("Only date before today is allowed");
            }

            station.StationId = Convert.ToInt16(stationId);
            UploadedFile file = station.ToUploadedFile();
            file.FileType = (int)UploadedFileType.FineData;

            if (await _fileService.FileExists(file))
            {
                return BadRequest("File already uploaded");
            }

            try
            {
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
            if(obj.DateTime.Date != DateTime.Today)
            {
                return BadRequest("Only today's data is allowed");
            }

            //Check station code
            obj.StationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());

            List<LoadData> validData = await this.CheckValidData(obj.StationId, new List<LoadData> { obj });
            if (validData.Count > 0)
            {
                bool isSuccess = await _axleLoadService.Add(validData);
                if(isSuccess)
                {
                    return Ok("Axle load data insert successful");
                }
                return BadRequest("Error: Axle load data insert failed");
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
            //if (obj.Any(l => l.DateTime.Date != DateTime.Today))
            //{
            //    return BadRequest("Only today's data is allowed");
            //}

            //Check station code
            int stationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());

            List<LoadData> validData = await this.CheckValidData(stationId, obj);
            
            if (validData.Count > 0)
            {
                foreach (var item in validData)
                {
                    item.StationId = stationId;
                }

                bool isSuccess = await _axleLoadService.Add(validData);
                if (isSuccess)
                {
                    return Ok("Axle load multiple data insert successful");
                }
                return BadRequest("Error: Axle load multiple data insert failed");
            }

            return BadRequest("Error: Axle load multiple data validation failed");
        }
        private async Task<List<LoadData>> CheckValidData(int stationId, List<LoadData> data)
        {
            //Data check
            data.RemoveAll(d => d.DateTime.Date != DateTime.Today);
            
            //Check lane number
            IEnumerable<WIMScale> wims = await _wimScaleService.GetByStation(new WIMScale(){ StationId = stationId });
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
            if (obj.DateTime.Date != DateTime.Today)
            {
                return BadRequest("Only today's data is allowed");
            }

            //Check station code
            obj.StationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());

            bool isSuccess = await _finePaymentService.Add(obj);
            if (isSuccess)
            {
                return Ok("Fine payment data insert successful");
            }
            return BadRequest("Error: Fine payment data insert failed");
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> FinePaymentMultiple(List<FinePayment> obj)
        {
            if (obj == null)
            {
                return new BadRequestResult();
            }
            if (obj.Any(l => l.DateTime.Date != DateTime.Today))
            {
                return BadRequest("Only today's data is allowed");
            }

            //Check station code
            int stationId = Convert.ToInt32(this.HttpContext.Request.Headers["StationId"].ToString());

            foreach(var item in obj)
            {
                item.StationId = stationId;
            }

            bool isSuccess = await _finePaymentService.Add(obj);
            if (isSuccess)
            {
                return Ok("Fine payment multiple data insert successful");
            }
            return BadRequest("Error: Fine payment multiple data insert failed");
        }
    }
}

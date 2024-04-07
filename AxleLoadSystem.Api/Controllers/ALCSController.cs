using AxleLoadSystem.Api.Extensions;
using AxleLoadSystem.Api.Models;
using BOL;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace AxleLoadSystem.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ALCSController : ControllerBase
    {
        private readonly IFileService _fileService;
        public ALCSController(IFileService fileService)
        {
            _fileService = fileService;
        }

	    [CustomAuthorize]
        [DisableRequestSizeLimit]
        //[ServiceFilter(typeof(ModelValidationAttribute))]
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadLoadData([ModelBinder(BinderType = typeof(JsonModelBinder))] FileUploadModel station, IFormFile uploadFile)
        {
            if (uploadFile == null || uploadFile.Length == 0 || station == null)
            {
                return new BadRequestResult();
            }

            //Check station code
            string stationId = this.HttpContext.Request.Headers["Station"].ToString();
            //string apiKey = this.HttpContext.Response.Headers["ApiKey"].ToString();
            //string key = this.HttpContext.Response.Headers.Authorization[0].ToString();

            if (Convert.ToInt16(stationId) != station.StationId)
            {
                return BadRequest("Station Id doesn't match");
            }

            station.StationId = Convert.ToInt16(stationId);
            UploadedFile file = station.ToUploadedFile();
            file.FileType = (int)UploadedFileType.LoadData;

            file = await this.UploadFile(file, uploadFile);

            return Ok(file.Id > 0);
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
            
            //Check station code
            string stationId = this.HttpContext.Request.Headers["Station"].ToString();
            //string apiKey = this.HttpContext.Response.Headers["ApiKey"].ToString();
            //string key = this.HttpContext.Response.Headers.Authorization[0].ToString();

            if (Convert.ToInt16(stationId) != station.StationId)
            {
                return BadRequest("Station Id doesn't match");
            }

            station.StationId = Convert.ToInt16(stationId);
            UploadedFile file = station.ToUploadedFile();
            file.FileType = (int)UploadedFileType.FineData;

            file = await this.UploadFile(file, uploadFile);

            return Ok(file.Id > 0);
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

        [HttpPost("[action]")]
        public async Task<IActionResult> SlowMoving(LoadDataSlowMoving obj)
        {
            if (obj == null)
            {
                return new BadRequestResult();
            }
            
            //await _fileService.Add(obj);

            return Ok(true);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> SlowMovingMultiple(List<LoadDataSlowMoving> obj)
        {
            if (obj == null)
            {
                return new BadRequestResult();
            }

            //await _fileService.Add(obj);

            return Ok(true);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> FastMoving(LoadDataFastMoving obj)
        {
            if (obj == null)
            {
                return new BadRequestResult();
            }

            //await _fileService.Add(obj);

            return Ok(true);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> FastMovingMultiple(List<LoadDataFastMoving> obj)
        {
            if (obj == null)
            {
                return new BadRequestResult();
            }

            //await _fileService.Add(obj);

            return Ok(true);
        }
    }
}

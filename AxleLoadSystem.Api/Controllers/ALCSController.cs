using AxleLoadSystem.Api.Extensions;
using AxleLoadSystem.Api.Models;
using BOL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Services;

namespace AxleLoadSystem.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ALCSController : ControllerBase
    {
        private IFileService _fileService;
        public ALCSController(IFileService fileService)
        {
            _fileService = fileService;
        }

	    [CustomAuthorize]
        [DisableRequestSizeLimit]
        //[ServiceFilter(typeof(ModelValidationAttribute))]
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload([ModelBinder(BinderType = typeof(JsonModelBinder))] FileUploadModel station, IFormFile uploadFile)
        {
            //if (this.HttpContext.Response.Headers.ContainsKey("StationCode"))
            //{
            //    return new UnauthorizedResult();
            //}
            if (uploadFile == null || uploadFile.Length == 0 || station == null)
            {
                return new BadRequestResult();
            }

            //Check station code
            string stationId  = this.HttpContext.Request.Headers["Station"].ToString();
            //string apiKey = this.HttpContext.Response.Headers["ApiKey"].ToString();
            //string key = this.HttpContext.Response.Headers.Authorization[0].ToString();
            
            if(Convert.ToInt16(stationId) != station.StationId)
            {
                return BadRequest("Station Id doesn't match");
            }

            byte[] byteFile;
            using (MemoryStream ms = new MemoryStream())
            {
                uploadFile.CopyTo(ms);
                byteFile = ms.ToArray();
            }

            UploadedFile file = new UploadedFile();
            //file.FileName = "S" + station.StationId + "_" + station.Date.ToString("yyyyMMdd");
            file.FileName = uploadFile.FileName;
            file.StationId = station.StationId;
            file.Date = station.Date;
            file.ManualUpload = false;
            
            await _fileService.Upload(byteFile, file);

            return Ok(true);
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

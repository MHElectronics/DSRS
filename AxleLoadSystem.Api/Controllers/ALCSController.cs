using AxleLoadSystem.Api.Extensions;
using AxleLoadSystem.Api.Models;
using BOL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Services;

namespace AxleLoadSystem.Api.Controllers
{
    [ApiController]
	[CustomAuthorize]
    [Route("[controller]")]
    public class ALCSController : ControllerBase
    {
        private FileService _fileService;
        public ALCSController(FileService fileService)
        {
            _fileService = fileService;
        }

        [DisableRequestSizeLimit]
        //[ServiceFilter(typeof(ModelValidationAttribute))]
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload([ModelBinder(BinderType = typeof(JsonModelBinder))] FileUploadModel station, IFormFile uploadFile)
        {
            if (this.HttpContext.Response.Headers.ContainsKey("StationCode"))
            {
                return new UnauthorizedResult();
            }
            if (uploadFile == null || uploadFile.Length == 0 || station == null)
            {
                return new BadRequestResult();
            }

            //Check station code
            string stationCode  = this.HttpContext.Response.Headers["StationCode"].ToString();
            string key = this.HttpContext.Response.Headers.Authorization[0].ToString();
            
            byte[] byteFile;
            using (MemoryStream ms = new MemoryStream())
            {
                uploadFile.CopyTo(ms);
                byteFile = ms.ToArray();
            }

            Files file = new Files();
            file.StationId = station.StationId;
            file.Date = station.Date;
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
            
            await _fileService.Add(obj);

            return Ok(true);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> SlowMovingMultiple(List<LoadDataSlowMoving> obj)
        {
            if (obj == null)
            {
                return new BadRequestResult();
            }

            await _fileService.Add(obj);

            return Ok(true);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> FastMoving(LoadDataFastMoving obj)
        {
            if (obj == null)
            {
                return new BadRequestResult();
            }

            await _fileService.Add(obj);

            return Ok(true);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> FastMovingMultiple(List<LoadDataFastMoving> obj)
        {
            if (obj == null)
            {
                return new BadRequestResult();
            }

            await _fileService.Add(obj);

            return Ok(true);
        }
    }
}

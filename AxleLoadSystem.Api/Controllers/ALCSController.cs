using AxleLoadSystem.Api.Extensions;
using BOL;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace AxleLoadSystem.Api.Controllers
{
    [ApiController]
	[CustomAuthorize]
    [Route("[controller]")]
    public class ALCSController : ControllerBase
    {
        private ALCSFilesService _fileService;
        public ALCSController(ALCSFilesService fileService)
        {
            _fileService = fileService;
        }

        [DisableRequestSizeLimit]
        //[ServiceFilter(typeof(ModelValidationAttribute))]
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ALCSFiles alcsFile, IFormFile uploadFile)
        {
            if (uploadFile == null || uploadFile.Length == 0 || alcsFile == null)
            {
                return new BadRequestResult();
            }

            //FileName = FileName from uploaded file information if FileName is missing
            if (string.IsNullOrEmpty(alcsFile.FileName))
            {
                string fileName = uploadFile.FileName;
            }
            
            byte[] byteFile;
            using (MemoryStream ms = new MemoryStream())
            {
                uploadFile.CopyTo(ms);
                byteFile = ms.ToArray();
            }

            await _fileService.Upload(alcsFile, byteFile);

            return Ok(true);
        }

    }
}

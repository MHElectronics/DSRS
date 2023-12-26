using BOL;

namespace Services
{
    public class ALCSFilesService
    {
        private FtpHelperService _ftpService;
        public ALCSFilesService(FtpHelperService ftService)
        {
            _ftpService = ftService;
        }
 
        public async Task Upload(ALCSFiles alcsFiles, byte[] byteFile)
        {
            //await _ftpService.UploadFile()
        }
    }
}

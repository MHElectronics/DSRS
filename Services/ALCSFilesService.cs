using BOL;

namespace Services
{
    public interface IALCSFilesService
    {
        Task Upload(ALCSFiles alcsFiles, byte[] byteFile);
    }
    public class ALCSFilesService : IALCSFilesService
    {
        private IFtpHelperService _ftpService;
        public ALCSFilesService(IFtpHelperService ftService)
        {
            _ftpService = ftService;
        }

        public async Task Upload(ALCSFiles alcsFiles, byte[] byteFile)
        {
            //await _ftpService.UploadFile()
        }
    }
}

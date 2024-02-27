using BOL;
using Services.Helpers;

namespace Services
{
    public interface IALCSFilesService
    {
        Task Upload(ALCSFiles alcsFiles, byte[] byteFile);
    }
    public class ALCSFilesService : IALCSFilesService
    {
        private IFtpHelper _ftpHelper;
        public ALCSFilesService(IFtpHelper ftpHelper)
        {
            _ftpHelper = ftpHelper;
        }

        public async Task Upload(ALCSFiles alcsFiles, byte[] byteFile)
        {
            //await _ftpHelper.UploadFile()
        }
    }
}

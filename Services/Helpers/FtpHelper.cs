using BOL;
using Microsoft.Extensions.Configuration;
using System.Drawing.Imaging;
using System.Data;
using System.Net;
using System.Runtime.Versioning;
using System.Drawing;

namespace Services.Helpers
{
    public interface IFtpHelper
    {
        Task<bool> UploadFile(byte[] byteFile, string fileName, string path = "");
        Task<bool> FileExists(string path);
        Task<byte[]> DownloadFile(string path);
        Task DeleteFile(string path);

        Task<string> Rename(string path = "", string renamePath = "");
        Task<string> MakeDirectory(string path = "");
        Task<bool> DirectoryExists(string path = "");
        Task<string> DeleteDirectory(string path = "");
        Task<bool> UploadFileInParts(byte[] byteFile, string path);
        Task<(bool isSuccess, DataTable? csvData, string summary)> GetDataTableFromCSV(string path, UploadedFile file);
        byte[] DownloadImageThumb(string path, int height = 100, int width = 100);
    }
    /// <summary>
    /// Ftp Helper class to handle all ftp requests
    /// </summary>
    public class FtpHelper(IConfiguration config, ICsvHelper csvHelper) : IFtpHelper
    {
        private readonly string _ftpAddress = config.GetSection("FtpAccess:Address").Value ?? "";
        private readonly string _ftpRootFolder = config.GetSection("FtpAccess:RootFolder").Value ?? "";
        private readonly string _ftpUser = config.GetSection("FtpAccess:User").Value ?? "";
        private readonly string _ftpPassword = config.GetSection("FtpAccess:Password").Value ?? "";

        private FtpWebRequest _ftpRequest;
        
        /// <summary>
        /// Open FTP Connection
        /// </summary>
        private void OpenConnection(string method, string path, string renamePath = "")
        {
            if (string.IsNullOrEmpty(_ftpUser) || string.IsNullOrEmpty(_ftpPassword) || string.IsNullOrEmpty(_ftpAddress))
            {
                throw new Exception("Ftp authentication information not fount");
            }

            string uri = _ftpAddress.TrimEnd('/') + "/";// + _ftpRootFolder.Trim('/') + "/";

            if (!string.IsNullOrEmpty(_ftpRootFolder.Trim('/')))
            {
                uri += _ftpRootFolder.Trim('/') + "/";
            }

            if (!string.IsNullOrEmpty(path))
            {
                uri += path.Trim('/');
            }

            //Create FTP request
            _ftpRequest = (FtpWebRequest)WebRequest.Create(uri);

            //Now get the actual data
            _ftpRequest.Method = method;
            _ftpRequest.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
            _ftpRequest.UsePassive = true;
            _ftpRequest.UseBinary = true;
            _ftpRequest.KeepAlive = false; //close the connection when done

            //Rename file or directory
            if (method == WebRequestMethods.Ftp.Rename && !string.IsNullOrEmpty(renamePath))
            {
                _ftpRequest.RenameTo = renamePath;
            }
        }

        #region File Functions
        public async Task<bool> UploadFile(byte[] byteFile, string fileName, string path = "")
        {
            //return falase if file size is 0
            if (byteFile.Length == 0)
            {
                return false;
            }

            path = path.TrimEnd('/') + "/" + fileName;

            OpenConnection(WebRequestMethods.Ftp.UploadFile, path);

            // Notify the server about the size of the uploaded file
            _ftpRequest.ContentLength = byteFile.Length;
            try
            {
                // Stream to which the file to be upload is written
                Stream responseStream = await _ftpRequest.GetRequestStreamAsync();
                responseStream.Write(byteFile, 0, byteFile.Length);

                responseStream.Close();
            }
            catch (Exception ex)
            {
                return false;
            }


            return true;
        }
        public async Task<bool> FileExists(string path)
        {
            //Open connection
            OpenConnection(WebRequestMethods.Ftp.GetDateTimestamp, path);

            //Get response
            try
            {
                FtpWebResponse response = (FtpWebResponse)await _ftpRequest.GetResponseAsync();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    //Does not exist
                    return false;
                }
            }

            return true;
        }

        public async Task<byte[]> DownloadFile(string path)
        {
            OpenConnection(WebRequestMethods.Ftp.DownloadFile, path);

            FtpWebResponse response = (FtpWebResponse)await _ftpRequest.GetResponseAsync();
            Stream reader = response.GetResponseStream();

            //Download to memory
            //Note: adjust the streams here to download directly to the hard drive
            MemoryStream memStream = new MemoryStream();
            byte[] buffer = new byte[1024]; //downloads in chuncks

            while (true)
            {
                //Try to read the data
                int bytesRead = reader.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    //Nothing was read, finished downloading
                    break;
                }
                else
                {
                    //Write the downloaded data
                    memStream.Write(buffer, 0, bytesRead);
                }
            }

            byte[] byteData = memStream.ToArray();

            //Clean up
            reader.Close();
            memStream.Close();
            response.Close();

            return byteData;
        }

        public async Task DeleteFile(string path)
        {
            try 
            {
                OpenConnection(WebRequestMethods.Ftp.DeleteFile, path);

                FtpWebResponse response = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

                response.Close();
            } 
            catch (Exception ex) 
            {
                throw ex; 
            }

        }
        #endregion

        public async Task<string> Rename(string path = "", string renamePath = "")
        {
            if (string.IsNullOrEmpty(renamePath))
            {
                throw new Exception("No new name found to rename");
            }

            string statusDescription = "";

            //Open connection
            OpenConnection(WebRequestMethods.Ftp.Rename, path, renamePath);

            try
            {
                //Get response
                using (FtpWebResponse response = (FtpWebResponse)await _ftpRequest.GetResponseAsync())
                {
                    statusDescription = response.StatusDescription;
                    response.Close();
                }

                return statusDescription;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        #region Director/Folder functions
        public async Task<string> MakeDirectory(string directory = "")
        {
            string statusDescription = "";

            //Make full path
            string fullPath = "/" + directory.Trim('/');

            //Open connection
            OpenConnection(WebRequestMethods.Ftp.MakeDirectory, fullPath);

            //Get response
            using (FtpWebResponse response = (FtpWebResponse)await _ftpRequest.GetResponseAsync())
            {
                statusDescription = response.StatusDescription;
                response.Close();
            }

            return statusDescription;
        }
        public async Task<bool> DirectoryExists(string directory = "")
        {
            //Make full path
            //Add \ at the end to make sure ListDirectory runs properly
            string fullPath = "/" + directory.Trim('/') + "/";

            //Open connection
            OpenConnection(WebRequestMethods.Ftp.ListDirectory, fullPath);

            //Get response
            try
            {
                using FtpWebResponse response = (FtpWebResponse)await _ftpRequest.GetResponseAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<string> DeleteDirectory(string directory = "")
        {
            string statusDescription = "";

            //Make full path
            string fullpath = "/" + directory;

            //Open connection
            OpenConnection(WebRequestMethods.Ftp.RemoveDirectory, fullpath);

            //Get response
            try
            {
                using (FtpWebResponse response = (FtpWebResponse)await _ftpRequest.GetResponseAsync())
                {
                    statusDescription = response.StatusDescription;
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }


            return statusDescription;
        }
        #endregion
        public async Task<bool> UploadFileInParts(byte[] byteFile, string path)
        {
            //return false if file size is 0
            if (byteFile.Length == 0)
            {
                return false;
            }

            OpenConnection(WebRequestMethods.Ftp.AppendFile, path);

            // Notify the server about the size of the uploaded file
            _ftpRequest.ContentLength = byteFile.Length;

            // Stream to which the file to be upload is written
            Stream responseStream = await _ftpRequest.GetRequestStreamAsync();
            await responseStream.WriteAsync(byteFile, 0, byteFile.Length);

            responseStream.Close();

            return true;
        }
        [SupportedOSPlatform("windows")]
        public byte[]? DownloadImageThumb(string path, int height = 100, int width = 100)
        {
            OpenConnection(WebRequestMethods.Ftp.DownloadFile, path);

            try
            {
                using FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse();
                using Stream reader = response.GetResponseStream();

                byte[] byteData;
                using (var image = System.Drawing.Image.FromStream(reader, true, true)) // Creates Image from the data stream
                {
                    using (Bitmap thumb = new Bitmap(width, height))
                    {
                        using (Graphics g = Graphics.FromImage(thumb))
                        {
                            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                            g.DrawImage(image, 0, 0, width, height); // Manually resizing the image
                        }

                        using (MemoryStream memStream = new MemoryStream())
                        {
                            ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg")!;
                            thumb.Save(memStream, myImageCodecInfo, null);
                            byteData = memStream.ToArray();
                        }
                    }
                }

                // Clean up
                response.Close();
                return byteData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                {
                    return codec;
                }
            }
            return null!;
        }
        public async Task<(bool isSuccess, DataTable? csvData, string summary)> GetDataTableFromCSV(string path, UploadedFile file)
        {
            byte[] streamInByte = await this.DownloadFile(path);
            
            if(streamInByte is not null)
            {
                return csvHelper.GetDataTableFromByte(streamInByte, file);
            }

            return (false, null, "");
        }
    }
}

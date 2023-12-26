using System.Net;

namespace Services
{
    public interface IFtpHelperService
    {
        bool UploadFile(byte[] byteFile, string fileName, string path = "");
        bool FileExists(string path);
        byte[] DownloadFile(string path); 
        void DeleteFile(string path);

        string Rename(string path = "", string renamePath = "");
        string MakeDirectory(string path = "");
        bool DirectoryExists(string path = "");
        string DeleteDirectory(string path = "");
    }
    /// <summary>
    /// Ftp Helper class to handle all ftp requests
    /// </summary>
    public class FtpHelperService: IFtpHelperService
    {
        private FtpWebRequest _ftpRequest;

        /// <summary>
        /// Subsction Object used for authentication
        /// </summary>

        private void OpenConnection(string method, string path, string renamePath = "")
        {
            string ftpUser = "";
            string ftpPassword = "";
            string ftpAddress = "";
            if(string.IsNullOrEmpty(ftpUser) || string.IsNullOrEmpty(ftpPassword) || string.IsNullOrEmpty(ftpAddress))
            {
                throw new Exception("Ftp authentication information not fount");
            }

            string uri = ftpAddress;

            if (!string.IsNullOrEmpty(path))
            {
                uri = uri + path.Trim('/');
            }

            //Create FTP request
            _ftpRequest = (FtpWebRequest)WebRequest.Create(uri);

            //Now get the actual data
            _ftpRequest.Method = method;
            _ftpRequest.Credentials = new NetworkCredential(ftpUser, ftpPassword);
            _ftpRequest.UsePassive = true;
            _ftpRequest.UseBinary = true;
            _ftpRequest.KeepAlive = false; //close the connection when done

            //Rename file or directory
            if(method == WebRequestMethods.Ftp.Rename && !string.IsNullOrEmpty(renamePath))
            {
                _ftpRequest.RenameTo = renamePath;
            }
        }

        #region File Functions
        public bool UploadFile(byte[] byteFile, string fileName, string path = "")
        {
            //return falase if file size is 0
            if (byteFile.Length == 0)
            {
                return false;
            }

            path = path.TrimEnd('/') + "/" + fileName;

            this.OpenConnection(WebRequestMethods.Ftp.UploadFile, path);

            // Notify the server about the size of the uploaded file
            _ftpRequest.ContentLength = byteFile.Length;

            // Stream to which the file to be upload is written
            Stream responseStream = _ftpRequest.GetRequestStream();
            responseStream.Write(byteFile, 0, byteFile.Length);

            responseStream.Close();

            return true;
        }
        public bool FileExists(string path)
        {
            //Open connection
            this.OpenConnection(WebRequestMethods.Ftp.GetDateTimestamp, path);

            //Get response
            try
            {
                using FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse();
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

        public byte[] DownloadFile(string path)
        {
            this.OpenConnection(WebRequestMethods.Ftp.DownloadFile, path);

            FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse();
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

        public void DeleteFile(string path)
        {
            this.OpenConnection(WebRequestMethods.Ftp.DeleteFile, path);

            FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse();

            response.Close();
        }
        #endregion

        public string Rename(string path = "", string renamePath = "")
        {
            if (string.IsNullOrEmpty(renamePath))
            {
                throw new Exception("No new name found to rename");
            }

            string statusDescription = "";

            //Open connection
            this.OpenConnection(WebRequestMethods.Ftp.Rename, path, renamePath);

            try
            {
                //Get response
                using (FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse())
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
        public string MakeDirectory(string directory = "")
        {
            string statusDescription = "";
            
            //Make full path
            string fullPath = "/" + directory;
            
            //Open connection
            this.OpenConnection(WebRequestMethods.Ftp.MakeDirectory, fullPath);

            //Get response
            using (FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse())
            {
                statusDescription = response.StatusDescription;
                response.Close();
            }

            return statusDescription;
        }
        public bool DirectoryExists(string directory = "")
        {
            //Make full path
            //Add \ at the end to make sure ListDirectory runs properly
            string fullPath = "/" + directory + "/";

            //Open connection
            this.OpenConnection(WebRequestMethods.Ftp.ListDirectory, fullPath);

            //Get response
            try
            {
                using FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        public string DeleteDirectory(string directory = "")
        {
            string statusDescription = "";

            //Make full path
            string fullpath = "/" + directory;

            //Open connection
            this.OpenConnection(WebRequestMethods.Ftp.RemoveDirectory, fullpath);

            //Get response
            try
            {
                using (FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse())
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
    }
}

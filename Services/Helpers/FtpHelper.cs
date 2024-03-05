using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Net;
using System.Reflection.PortableExecutable;

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

        Task<DataTable> GetDataTabletFromCSVFile(string path);
    }
    /// <summary>
    /// Ftp Helper class to handle all ftp requests
    /// </summary>
    public class FtpHelper : IFtpHelper
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
            if (string.IsNullOrEmpty(ftpUser) || string.IsNullOrEmpty(ftpPassword) || string.IsNullOrEmpty(ftpAddress))
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

            // Stream to which the file to be upload is written
            Stream responseStream = await _ftpRequest.GetRequestStreamAsync();
            responseStream.Write(byteFile, 0, byteFile.Length);

            responseStream.Close();

            return true;
        }
        public async Task<bool> FileExists(string path)
        {
            //Open connection
            OpenConnection(WebRequestMethods.Ftp.GetDateTimestamp, path);

            //Get response
            try
            {
                using FtpWebResponse response = (FtpWebResponse)await _ftpRequest.GetResponseAsync();
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
            OpenConnection(WebRequestMethods.Ftp.DeleteFile, path);

            FtpWebResponse response = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

            response.Close();
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
            string fullPath = "/" + directory;

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
            string fullPath = "/" + directory + "/";

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

        public async Task<DataTable> GetDataTabletFromCSVFile(string path)
        {
            OpenConnection(WebRequestMethods.Ftp.DownloadFile, path);

            FtpWebResponse response = (FtpWebResponse)await _ftpRequest.GetResponseAsync();
            Stream responseStream = response.GetResponseStream();

            //StreamReader reader = new StreamReader(responseStream);
            //string[] allLines = reader.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            DataTable csvData = new DataTable();
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(responseStream))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            //Clean up
            responseStream.Close();
            //reader.Close();
            response.Close();
            
            return csvData;
        }
    }
}

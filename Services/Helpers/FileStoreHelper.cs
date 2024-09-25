using BOL.Models;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Services.Helpers;

public interface IFileStoreHelper
{
    Task<bool> CreateDocumentDirectory(Document document);

    Task<string> GetImageContentAsync(Document document);
    string GetImageThumb(Document document);

    string GetNewFileLocation(string fileName, Document document);

    Task<string> UploadFile(byte[] fileBytes, string fileName, Document document);
    Task UploadFileInPartsAsync(byte[] fileBytes, string path);

    void DeleteFile(string path);
}

public class FileStoreHelper(IConfiguration config,IFtpHelper ftpHelper) : IFileStoreHelper
{
    private bool _isFtpEnabled = false;

    private readonly string _ftpAddress = config.GetSection("FtpAccess:Address").Value ?? "";
    private readonly string _ftpRootFolder = config.GetSection("FtpAccess:RootFolder").Value ?? "";
    private readonly string _ftpUser = config.GetSection("FtpAccess:User").Value ?? "";
    private readonly string _ftpPassword = config.GetSection("FtpAccess:Password").Value ?? "";

    public async Task<bool> CreateDocumentDirectory(Document document)
    {
        string folderPath = this.GetDocumentDirectoryPath(document);

        if (_isFtpEnabled)
        {
            string[] folders = folderPath.Split('/');
            string startPath = "";
            for (int i = 0; i < folders.Length; i++)
            {
                string newPath = startPath + "/" + folders[i];
                if (await ftpHelper.DirectoryExists(newPath) == false)
                {
                    ftpHelper.MakeDirectory(newPath);
                }
                startPath += "/" + folders[i];
            }
        }
        else
        {
            string rootPath = this.GetRootPath();
            Directory.CreateDirectory(Path.Combine(rootPath, folderPath));
        }

        return true;
    }
    private string GetRootPath()
    {
        if (_isFtpEnabled)
        {
            //return config.GetValue<string>("FtpSettings:Address") ?? "";
            return "";
        }
        else
        {
            string rootFolderPath = _ftpRootFolder;
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", rootFolderPath);
        }
    }
    private string GetDocumentDirectoryPath(Document document)
    {
        if (_isFtpEnabled)
        {
            return document.Date.ToString("yyyy") + "_" + document.Date.ToString("MM") + "/D_" + document.Id;
        }
        else
        {
            return Path.Combine(document.Date.ToString("yyyy") + "_" + document.Date.ToString("MM"), "C_" + document.Id);
        }
    }

    public async Task<string> GetImageContentAsync(Document document)
    {
        if (_isFtpEnabled)
        {
            try
            {
                string path = document.FileLocation;
                byte[] bytes = await ftpHelper.DownloadFile(path);
                return "data:image/png;base64, " + Convert.ToBase64String(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                throw new Exception("Image content did not found" + ex);
            }
        }
        else
        {
            string rootFolderPath = _ftpRootFolder;
            string referencePath = Path.Combine(rootFolderPath, document.FileLocation);
            return referencePath;
        }
    }
    public string GetImageThumb(Document document)
    {
        try
        {
            if (_isFtpEnabled)
            {
                string path = document.FileLocation;
                byte[] bytes = ftpHelper.DownloadImageThumb(path);
                if (bytes is not null)
                {
                    return "data:image/png;base64, " + Convert.ToBase64String(bytes, 0, bytes.Length);
                }

                return "";
            }
            else
            {
                string rootFolderPath = _ftpRootFolder;
                string referencePath = Path.Combine(rootFolderPath, document.FileLocation);
                return referencePath;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Image file corrupted" + ex);
        }  
    }
    public async Task<string> UploadFile(byte[] fileBytes, string fileName, Document document)
    {
        //Generate random file name
        string newFileName = Path.ChangeExtension(
                Path.GetRandomFileName(),
                Path.GetExtension(fileName));

        //File path
        string folderPath = this.GetDocumentDirectoryPath(document);
        string fileLocation = Path.Combine(folderPath, newFileName);

        //Upload file
        if (_isFtpEnabled)
        {
            fileLocation = folderPath + "/" + newFileName;
            await ftpHelper.UploadFile(fileBytes, fileLocation);
        }
        else
        {
            string fullPath = Path.Combine(this.GetRootPath(), fileLocation);
            await using FileStream writeStream = new(fullPath, FileMode.Create);
            await writeStream.WriteAsync(fileBytes, 0, fileBytes.Length);

        }

        return fileLocation;
    }
    public string GetNewFileLocation(string fileName, Document document)
    {
        //Generate random file name
        string newFileName = Path.ChangeExtension(
                Path.GetRandomFileName(),
                Path.GetExtension(fileName));

        //File path
        string folderPath = this.GetDocumentDirectoryPath(document);
        string fileLocation = Path.Combine(folderPath, newFileName);

        return fileLocation;
    }
    public async Task UploadFileInPartsAsync(byte[] fileBytes, string path)
    {
        if (_isFtpEnabled)
        {
            await ftpHelper.UploadFileInParts(fileBytes, path);
        }
        else
        {
            string fullPath = Path.Combine(this.GetRootPath(), path);
            using FileStream writeStream = new(fullPath, FileMode.Append);
            await writeStream.WriteAsync(fileBytes, 0, fileBytes.Length);
            writeStream.Close();
        }
    }
    public void DeleteFile(string path)
    {
        if (_isFtpEnabled)
        {
            ftpHelper.DeleteFile(path);
        }
        else
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}

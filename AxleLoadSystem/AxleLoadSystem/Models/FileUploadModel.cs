using BOL;

namespace DSRSystem.Models;

public class FileUploadModel : UploadedFile
{
    public IFormFileCollection Attachments { get; set; }
}

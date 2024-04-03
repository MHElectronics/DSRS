using BOL;

namespace AxleLoadSystem.Models;

public class FileUploadModel : UploadedFile
{
    public IFormFileCollection Attachments { get; set; }
}

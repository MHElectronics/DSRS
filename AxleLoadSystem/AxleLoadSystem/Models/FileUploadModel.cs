using BOL;

namespace AxleLoadSystem.Models;

public class FileUploadModel : Files
{
    public IFormFileCollection Attachments { get; set; }
}

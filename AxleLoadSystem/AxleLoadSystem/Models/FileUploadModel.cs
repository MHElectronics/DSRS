using BOL;

namespace AxleLoadSystem.Models;

public class FileUploadModel// : UploadedFile
{
    public int StationId { get; set; }
    public DateTime Date { get; set; }
    public IFormFileCollection Attachments { get; set; }
}

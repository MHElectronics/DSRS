using BOL;
using System.ComponentModel.DataAnnotations;

namespace DSRSystem.Api.Models;

public class FileUploadModel
{
    [Required]
    public int StationId { get; set; }
    [Required]
    public DateTime Date { get; set; }
    //[Required]
    //public int FileType {  get; set; }

    public UploadedFile ToUploadedFile()
    {
        UploadedFile file = new()
        {
            StationId = StationId,
            Date = Date
            //FileType = FileType
        };

        return file;
    }
}

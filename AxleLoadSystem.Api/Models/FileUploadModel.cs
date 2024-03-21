using System.ComponentModel.DataAnnotations;

namespace AxleLoadSystem.Api.Models;

public class FileUploadModel
{
    [Required]
    public int StationId { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public int FileType {  get; set; }
}

namespace BOL;
public class UploadedFile
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public DateTime Date { get; set; }
    public int FileType { get; set; }
    public string FileName { get; set; }
    public bool ManualUpload { get; set; }
    public DateTime UploadDate { get; set; }
    public bool IsProcessed { get; set; }
    public string Summary { get; set; }
}

namespace BOL.Models;

public class Documents
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FileLocation { get; set; }
    public string Description { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
}


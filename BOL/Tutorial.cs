namespace BOL.Models;

public class Tutorial
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FileLocation { get; set; }
    public string Description { get; set; }
    public int UserId { get; set; }
    public int DisplayOrder {  get; set; }
    public DateTime Date { get; set; }
    public int TutorialCategoryId { get; set; }
}


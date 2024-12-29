namespace BOL;
public class FAQ
{
    public int Id { get; set; }
    public string Question { get;set; }
    public string Answer { get; set; }
    public int QuestionUserId { get; set; }
    public int AnswerUserId { get; set; }
    public DateTime AnswerDateTime { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime EntryTime {  get; set; }
}

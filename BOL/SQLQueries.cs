namespace BOL;
public class SQLQueries
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Query { get; set; }
    public string Parameters { get; set; }
    
    public List<string> ParameterList
    {
        get { return this.Parameters.Split(",").ToList(); }
        set { this.Parameters = String.Join(",", value); }
    }
}
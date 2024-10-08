namespace BOL;
public class Station
{
    public int StationId { get; set; }
    public string StationName { get; set; }
    public string Address { get; set; }
    public string AuthKey { get; set; }
    public string MapX { get; set; }
    public string MapY { get; set; }
    public bool IsUpbound { get;set; }
}

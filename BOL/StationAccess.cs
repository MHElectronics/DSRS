namespace BOL;
public class StationAccess
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StationIds { get; set; }
    public DateTime EntryTime { get; set; }
    public int EntryBy { get; set; }

    public bool HasStationIds(string stationIds)
    { 
        if(string.IsNullOrEmpty(StationIds))
            return false;
        return StationIds.Split(',').Contains(stationIds.ToString(),StringComparer.OrdinalIgnoreCase);
    }
}

namespace BOL;
public class WIMScale
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public int LaneNumber { get; set; } = 1;
    public int Type {  get; set; }
    public string EquipmentCode { get; set; }
    public string LaneDirection { get; set; }
    public bool IsUpbound { get; set; }
}

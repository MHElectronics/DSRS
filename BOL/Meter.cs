namespace BOL;
public class Meter
{
    public int Id { get; set; }
    public string MeterNumber { get; set; }
    public string Address { get; set; }
    public MeterType MeterType { get; set; }
    public int CompanyId { get; set; }
    public int Dcuid { get; set; }
    public int CustomerId { get; set; }
    public bool IsActive { get; set; }
}

namespace BOL;

public class Configuration
{
    public int Id { get; set; }
    public int NumberOfAxle { get; set; }
    public DateTime SystemStartDate { get; set; } = DateTime.Now;
    public int WheelBaseMaximum { get; set; }
    public int WeightMinimum { get; set; }
    public int WeightMaximum { get; set; }
}

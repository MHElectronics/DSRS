namespace BOL.CustomModels;
public class ReportParameters
{
    public List<int> Stations { get; set; } = new();
    public int WIMScaleId { get; set; }
    public DateTime DateStart { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime DateEnd { get; set; } = DateTime.Today;
    public int NumberOfAxle { get; set; }
    public int ClassStatus { get; set; }
    public int Wheelbase { get; set; }
    public string WeightFilterColumn { get; set; } = String.Empty;
    public int WeightMin { get; set; }
    public int WeightMax { get; set; }
    public bool CheckWeightCalculation { get; set; }
    public bool IsOverloaded { get; set; }
    public int ChartType { get; set; }
    public int Multiplier { get; set; }
    public int FileType { get; set; }
}

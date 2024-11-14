namespace BOL.CustomModels;
public class ReportParameters
{
    public List<int> Stations { get; set; } = new();
    public List<WIMScale> WIMScales { get; set; }
    public bool UpboundDirection { get; set; }
    public bool DownboundDirection { get; set; }
    public int WIMType { get; set; } = 0;
    public DateTime DateStart { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime DateEnd { get; set; } = DateTime.Today;
    public TimeOnly TimeStart { get; set; } 
    public TimeOnly TimeEnd { get; set; } 
    public List<int> NumberOfAxle { get; set; }
    public int ClassStatus { get; set; }
    public int Wheelbase { get; set; }
    public string WeightFilterColumn { get; set; } = String.Empty;
    public int WeightMin { get; set; }
    public int WeightMax { get; set; }
    public int Multiplier { get; set; }
}

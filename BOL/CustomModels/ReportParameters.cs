using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOL.CustomModels;
public class ReportParameters
{
    public Station SelectedStation { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public int NumberOfAxle { get; set; }
    public int ClassStatus { get; set; }
    public int Wheelbase { get; set; }
    public string WeightFilterColumn { get; set; }
    public int WeightMin { get; set; }
    public int WeightMax { get; set; }
    public bool CheckWeightCalculation { get; set; }
    public bool IsOverloaded { get; set; }
}

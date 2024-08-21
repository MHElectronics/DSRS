using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOL.CustomModels;
public class ReportParameters
{
    public Station SelectedStation { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int AxleNumber { get; set; }
    public int ClassStatus { get; set; }
    public int WheelBase { get; set; }
    public bool IsOverloaded { get; set; }
}

using BOL.Helpers;

namespace BOL.CustomModels;
public class AxleLoadReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; } 
    public int NumberOfAxle { get; set; }
    public int Overload { get; set; }
    public int TotalVehicle { get; set; }
    public int Axle1 { get; set; }
    public int Axle2 { get; set; }
    public int Axle3 { get; set; }
    public int Axle4 { get; set; }
    public int Axle5 { get; set; }
    public int Axle6 { get; set; }
    public int Axle7 { get; set; }
    public int AxleRemaining { get; set; }
    public int Wheelbase { get; set; }
    public int ClassStatus { get; set; }

}

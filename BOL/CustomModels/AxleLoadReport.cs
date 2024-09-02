using BOL.Helpers;

namespace BOL.CustomModels;
public class AxleLoadReport
{
    public int Count { get; set; }
    public DateTime Date { get; set; }
    public int Month { get; set; }
    public int Weekday { get; set; }
    public int NumberofAxle { get; set; }
    public int Axle1 { get; set; }
    public int Axle2 { get; set; }
    public int Axle3 { get; set; }
    public int Axle4 { get; set; }
    public int Axle5 { get; set; }
    public int Axle6 { get; set; }
    public int Axle7 { get; set; }
    public int AxleRemaining { get; set; }
    public int GrossVehicleWeight { get; set; }
    public int TotalVehicles { get; set; }
    public int OverloadedVehicles { get; set; }

}

using BOL.Helpers;

namespace BOL.CustomModels;
public class AxleLoadReport
{
    public int TotalVehicle { get; set; }
    public int OverloadVehicle { get; set; }
    public int NotOverloadVehicle { get; set; } 
    public DateTime Date { get; set; }
    public int DateUnit { get; set; }
    public string DateUnitName { get; set; } = "";
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
    public string GrossVehicleWeightRange { get; set; } = string.Empty;
    public double OverloadingRatio { get; set; }

    // Additional Property for Overloaded Histogram for Influence Degree and Cumilative Number of Axles
    public int TotalNumberOfAxles { get; set; }
    public double MediumWeight { get; set; }
    public double Influence { get; set; }
    public double Influence_2 { get; set; }
}

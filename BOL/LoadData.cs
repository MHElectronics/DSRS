using BOL.Helpers;

namespace BOL;
public class LoadData
{
    public int StationId { get; set; }

    public string TransactionNumber { get; set; }
    public int StationCode { get; set; }
    public int LaneNumber { get; set; }
    [JsonDateTimeFormat]
    public DateTime DateTime { get; set; }
    public string PlateZone { get; set; }
    public string PlateSeries { get; set; }
    public string PlateNumber { get; set; }
    public string VehicleId { get; set; }
    public int NumberOfAxle { get; set; }
    public int Axle1 { get; set; }
    public int Axle2 { get; set; }
    public int Axle3 { get; set; }
    public int Axle4 { get; set; }
    public int Axle5 { get; set; }
    public int Axle6 { get; set; }
    public int Axle7 { get; set; }
    public int AxleRemaining { get; set; }
    public int GrossVehicleWeight { get; set; }
    public bool IsUnloaded { get; set; }
    public bool IsOverloaded { get; set; }
    public bool OverSizedModified { get; set; }
    public int Wheelbase { get; set; }
    public string ReceiptNumber { get; set; }
    public string BillNumber { get; set; }

    //Slow Moving
    public int VehicleSpeed { get; set; }
    [JsonDateTimeFormat]
    public DateTime? Axle1Time { get; set; }
    [JsonDateTimeFormat]
    public DateTime? Axle2Time { get; set; }
    [JsonDateTimeFormat]
    public DateTime? Axle3Time { get; set; }
    [JsonDateTimeFormat]
    public DateTime? Axle4Time { get; set; }
    [JsonDateTimeFormat]
    public DateTime? Axle5Time { get; set; }
    [JsonDateTimeFormat]
    public DateTime? Axle6Time { get; set; }
    [JsonDateTimeFormat]
    public DateTime? Axle7Time { get; set; }

    public DateTime EntryTime { get; set; }
}

using BOL.Helpers;

namespace BOL;
public class LoadData
{
    public int StationId { get; set; }

    public string TransactionNumber { get; set; }
    public int StationCode { get; set; }
    public int LaneNumber { get; set; }
    [JsonDateTimeFormat("dd/MM/yyyy HH:mm:ss")]
    public DateTime DateTime { get; set; }
    public string PlateZone { get; set; }
    public string PlateSeries { get; set; }
    public string PlateNumber { get; set; }
    public string VehicleId { get; set; }
    public int NumberOfAxle { get; set; }
    public decimal Axle1 { get; set; }
    public decimal Axle2 { get; set; }
    public decimal Axle3 { get; set; }
    public decimal Axle4 { get; set; }
    public decimal Axle5 { get; set; }
    public decimal Axle6 { get; set; }
    public decimal Axle7 { get; set; }
    public decimal AxleRemaining { get; set; }
    public decimal GrossVehicleWeight { get; set; }
    public decimal FinePayment { get; set; }
    //public string ReceiptNumber { get; set; }
    //public string BillNumber { get; set; }
    public bool IsUnloaded { get; set; }
    public bool IsOverloaded { get; set; }
    public bool OverSizedModified { get; set; }
    public decimal Wheelbase { get; set; }

    //Slow Moving
    public decimal VehicleSpeed { get; set; }
    [JsonDateTimeFormat("dd/MM/yyyy HH:mm:ss")]
    public DateTime? Axle1Time { get; set; }
    [JsonDateTimeFormat("dd/MM/yyyy HH:mm:ss")]
    public DateTime? Axle2Time { get; set; }
    [JsonDateTimeFormat("dd/MM/yyyy HH:mm:ss")]
    public DateTime? Axle3Time { get; set; }
    [JsonDateTimeFormat("dd/MM/yyyy HH:mm:ss")]
    public DateTime? Axle4Time { get; set; }
    [JsonDateTimeFormat("dd/MM/yyyy HH:mm:ss")]
    public DateTime? Axle5Time { get; set; }
    [JsonDateTimeFormat("dd/MM/yyyy HH:mm:ss")]
    public DateTime? Axle6Time { get; set; }
    [JsonDateTimeFormat("dd/MM/yyyy HH:mm:ss")]
    public DateTime? Axle7Time { get; set; }

    public DateTime EntryTime { get; set; }
}

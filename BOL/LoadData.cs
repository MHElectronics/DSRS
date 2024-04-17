namespace BOL;
public class LoadData
{
    public int StationId { get; set; }
    public int WIMId { get; set; }
    public int BRTAInfoId { get; set; }

    public string TransactionNumber { get; set; }
    public int StationCode { get; set; }
    public int LaneNumber { get; set; }
    public DateTime DateTime { get; set; }
    public string PlateZone { get; set; }
    public string PlateSeries { get; set; }
    public string PlateNumber { get; set; }
    public int VehicleId { get; set; }
    public int NumberOfAxle { get; set; }
    public decimal Axle1st { get; set; }
    public decimal Axle2nd { get; set; }
    public decimal Axle3rd { get; set; }
    public decimal Axle4th { get; set; }
    public decimal Axle5th { get; set; }
    public decimal Axle6th { get; set; }
    public decimal Axle7th { get; set; }
    public decimal AxleRemaining { get; set; }
    public decimal GrossVehicleWeight { get; set; }
    public decimal FinePayment { get; set; }
    public string ReceiptNumber { get; set; }
    public string BillNumber { get; set; }
    public bool IsUnloaded { get; set; }
    public bool IsOverloaded { get; set; }
    public bool OverSizedModified { get; set; }
    public decimal Wheelbase { get; set; }

    //Slow Moving
    public decimal VehicleSpeed { get; set; }
    public DateTime Axle1stTime { get; set; }
    public DateTime Axle2ndTime { get; set; }
    public DateTime Axle3rdTime { get; set; }
    public DateTime Axle4thTime { get; set; }
    public DateTime Axle5thTime { get; set; }
    public DateTime Axle6thTime { get; set; }
    public DateTime Axle7thTime { get; set; }

    public DateTime EntryTime { get; set; }
}

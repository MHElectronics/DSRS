namespace BOL;
public class LoadData
{
    public int StationId { get; set; }
    public int WIMId { get; set; }
    public int BRTAInfoId { get; set; }

    public int TransactionNumber { get; set; }
    public int StationCode { get; set; }
    public int LaneNumber { get; set; }
    public int DateTime { get; set; }
    public int PlateNumber { get; set; }
    public int VehicleId { get; set; }
    public int NumberOfAxle { get; set; }
    public int Axle1st { get; set; }
    public int Axle2nd { get; set; }
    public int Axle3rd { get; set; }
    public int Axle4th { get; set; }
    public int Axle5th { get; set; }
    public int Axle6th { get; set; }
    public int Axle7th { get; set; }
    public int AxleRemaining { get; set; }
    public int GrossVehicleWeight { get; set; }
    public int FinePayment { get; set; }
    public int ReceiptNumber { get; set; }
    public int BillNumber { get; set; }
    public int IsUnloaded { get; set; }
    public int IsOverloaded { get; set; }
    public int OverSizedModified { get; set; }
    public int Wheelbase { get; set; }

    //Slow Moving
    public decimal VehicleSpeed { get; set; }
    public DateTime Axle1stTime { get; set; }
    public DateTime Axle2ndTime { get; set; }
    public DateTime Axle3rdTime { get; set; }
    public DateTime Axle4thTime { get; set; }
    public DateTime Axle5thTime { get; set; }
    public DateTime Axle6thTime { get; set; }
    public DateTime Axle7thTime { get; set; }
}

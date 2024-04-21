using BOL.Helpers;

namespace BOL;
public class FinePayment
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public string TransactionNumber { get; set; }
    [JsonDateTimeFormat]
    public DateTime DateTime { get; set; }
    public bool IsPaid { get; set; }
    public decimal FineAmount { get; set; }
    public string PaymentMethod { get; set; }
    public string ReceiptNumber { get; set; }
    public string BillNumber { get; set; }
    public decimal WarehouseCharge { get; set; }
    public string DriversLicenseNumber { get; set; }
    public DateTime EntryTime { get; set; }
}

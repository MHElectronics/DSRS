using BOL.Helpers;

namespace BOL;
public class FinePayment
{
    public int Id { get; set; }
    public int StationId { get; set; }
    [NoSpecialCharacters]
    public string TransactionNumber { get; set; }
    [NoSpecialCharacters]
    public string PaymentTransactionId { get; set; }
    [JsonDateTimeFormat]
    public DateTime DateTime { get; set; }
    public bool IsPaid { get; set; }
    public decimal FineAmount { get; set; }
    public string PaymentMethod { get; set; }
    [NoSpecialCharacters]
    public string ReceiptNumber { get; set; }
    [NoSpecialCharacters]
    public string BillNumber { get; set; }
    public decimal WarehouseCharge { get; set; }
    public string DriversLicenseNumber { get; set; }
    public string TransportAgencyInformation { get; set; }
    public DateTime EntryTime { get; set; }
}

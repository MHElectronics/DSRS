using BOL.Helpers;
using System.ComponentModel.DataAnnotations;

namespace BOL;
public class FinePayment
{
    public int Id { get; set; }
    public int StationId { get; set; }
    [Required]
    [MinValue(0)]
    [MaxValue(99)]
    public int LaneNumber { get; set; }
    [NoSpecialCharacters]
    public string TransactionNumber { get; set; }
    [NoSpecialCharacters]
    public string PaymentTransactionId { get; set; }
    [JsonDateTimeFormat]
    [Required]
    public DateTime DateTime { get; set; }
    public bool IsPaid { get; set; }
    [MinValue(0)]
    public decimal FineAmount { get; set; }
    [NoSpecialCharacters]
    public string PaymentMethod { get; set; }
    [NoSpecialCharacters]
    public string ReceiptNumber { get; set; }
    [NoSpecialCharacters]
    public string BillNumber { get; set; }
    [MinValue(0)]
    public decimal WarehouseCharge { get; set; }
    public string DriversLicenseNumber { get; set; }
    public string TransportAgencyInformation { get; set; }
    public DateTime EntryTime { get; set; }
}

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
    public string TransactionNumber { get; set; } = string.Empty;
    [NoSpecialCharacters]
    public string PaymentTransactionId { get; set; } = string.Empty;
    [JsonDateTimeFormat]
    [Required]
    public DateTime DateTime { get; set; }
    public bool IsPaid { get; set; }
    [MinValue(0)]
    public decimal FineAmount { get; set; }
    [NoSpecialCharacters]
    public string PaymentMethod { get; set; } = string.Empty;
    [NoSpecialCharacters]
    public string ReceiptNumber { get; set; } = string.Empty;
    [NoSpecialCharacters]
    public string BillNumber { get; set; } = string.Empty;
    [MinValue(0)]
    public decimal WarehouseCharge { get; set; }
    public string DriversLicenseNumber { get; set; } = string.Empty;
    public string TransportAgencyInformation { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }
    //Additional properties
    public string DateString { get { return this.DateTime.ToString("yyyy/MM/dd"); } }
    public string TimeString { get { return this.DateTime.ToString("HH:mm:ss"); } }
}

using BOL.Helpers;
using System.ComponentModel.DataAnnotations;

namespace BOL;
public class LoadData
{
    public int StationId { get; set; }
    [NoSpecialCharacters]
    [Required]
    public string TransactionNumber { get; set; } = string.Empty;
    [Required]
    [MinValue(0)]
    [MaxValue(99)]
    public int LaneNumber { get; set; }
    
    [JsonDateTimeFormat]
    [Required]
    public DateTime DateTime { get; set; }
    public string PlateZone { get; set; } = string.Empty;
    public string PlateSeries { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    [MinValue(0)]
    [Required]
    public int NumberOfAxle { get; set; }

    [MinValue(0)]
    [Required]
    public int Axle1 { get; set; }
    [MinValue(0)]
    public int Axle2 { get; set; }
    [MinValue(0)]
    public int Axle3 { get; set; }
    [MinValue(0)]
    public int Axle4 { get; set; }
    [MinValue(0)]
    public int Axle5 { get; set; }
    [MinValue(0)]
    public int Axle6 { get; set; }
    [MinValue(0)]
    public int Axle7 { get; set; }
    [MinValue(0)]
    public int AxleRemaining { get; set; }
    [MinValue(0)]
    public int GrossVehicleWeight { get; set; }
    public bool IsUnloaded { get; set; }
    public bool IsOverloaded { get; set; }
    public bool OverSizedModified { get; set; }
    [MinValue(0)]
    public int Wheelbase { get; set; }
    [MinValue(0)]
    public int ClassStatus { get; set; }
    [MinValue(0)]
    public int RecognizedBy { get; set; }
    public bool IsBRTAInclude { get; set; }
    [MinValue(0)]
    public int LadenWeight { get; set; }
    [MinValue(0)]
    public int UnladenWeight { get; set; }
    [NoSpecialCharacters]
    public string ReceiptNumber { get; set; } = string.Empty;
    [NoSpecialCharacters]
    public string BillNumber { get; set; } = string.Empty;

    //Slow Moving
    [MinValue(0)]
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

    //Additional properties
    public string StationName { get; set; }
    public string DateString { get { return this.DateTime.ToString("yyyy/MM/dd"); } }
    public string TimeString { get { return this.DateTime.ToString("HH:mm:ss"); } }
}

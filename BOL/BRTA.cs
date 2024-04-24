namespace BOL;

public class Brta
{
    public RhdClass rhdClass { get; set; }
    public BrtaClass brtaClass { get; set; }
    public VehicleInfo vehicleInfo { get; set; }
    public int fareRate { get; set; }
    public string bridgeName { get; set; }
    public string bridgeOid { get; set; }
    public string message { get; set; }
    public string status { get; set; }
}
public class RhdClass
{
    public string className { get; set; }
    public string regSeries { get; set; }
    public string seriesBnName { get; set; }

}
public class BrtaClass
{ 
    public string className { get; set; }
    public string regSeries { get; set; }
    public string regSeriesBnName { get; set;}
    public decimal seriesStartNumber { get; set; }
    public decimal seriesEndNumber { get; set;}
}
public class VehicleInfo
{
    public string vehicleRegistrationNumber { get; set; }
    public string zoneOid { get; set; }
    public string seriesOid { get; set; }
    public string requestKey { get; set; }
    public string fatherHusbendName { get; set; }
    public string mobileNumber { get; set; }
    public DateTime responseTime { get; set; }
    public string noOfAxle { get; set; }
    public string jointOwner { get; set; }
    public string vehicleColour { get; set; }
    public string unladenWeight { get; set; }
    public double responseTimeEpoch { get; set; }
    public string vehicleCC { get; set; }
    public string ladenWeight { get; set; }
    public string ownerName { get; set; }
    public string nationality { get; set; }
    public int registrationNumber { get; set; }
    public int vehicleSeries { get; set; }
    public DateTime taxTokenExpDate { get; set; }
    public DateTime registrationDate { get; set; }
    public string vehicleNumber { get; set; }
    public string vehicleClass { get; set; }
    public string ownerAddress { get; set; }
    public DateOnly taxTokenIssueDate { get; set; }
    public string vehicleType { get; set; }
    public string seatingCapacity { get; set; }
    public string registrationOfficeName { get; set; }
    public string rfid { get; set; }
    public string nidNumber { get; set; }
    public DateTime dateOfBirth { get; set; }
    public string zone { get; set; }
    public string series { get; set; }
    public string routePermitNumber { get; set; }
    public DateTime routePermitIssueDate { get; set; }
    public DateTime fitnessIssueDate { get; set; }
    public DateTime fitnessExpDate { get; set; }
    public string jointOwnerName { get; set; }
    public string requestId { get; set; }
}

using BOL;
using BOL.CustomModels;
using Services.Helpers;
using System.Data.SqlClient;

namespace Services;

public interface IFinePaymentService
{
    Task<IEnumerable<FinePayment>> Get(FinePayment obj);
    Task<(IEnumerable<FinePayment>, bool, string)> Get(ReportParameters reportParameters);
    Task<(bool,string)> Add(FinePayment obj);
    Task<(bool,string)> Add(List<FinePayment> obj);
    Task<bool> Delete(UploadedFile file);
}

public class FinePaymentService(ISqlDataAccess _db) : IFinePaymentService
{
    public async Task<IEnumerable<FinePayment>> Get(FinePayment obj)
    {
        string query = @"SELECT TransactionNumber,LaneNumber,PaymentTransactionId 
        ,DateTime,IsPaid,FineAmount,PaymentMethod,ReceiptNumber,BillNumber 
        ,WarehouseCharge,DriversLicenseNumber,TransportAgencyInformation 
        FROM FinePayment
        WHERE StationId=@StationId AND DATEDIFF(DAY,DateTime,@DateTime)=0";

        return await _db.LoadData<FinePayment, object>(query, obj);
    }
    public async Task<(IEnumerable<FinePayment>, bool, string)> Get(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string stationIds = string.Join(",", reportParameters.Stations.Select(s => "(" + s + ")"));
        string laneNumbers = string.Join(",", reportParameters.WIMScales.Select(ws => "(" + ws.LaneNumber + ")"));
        string query = @"SELECT TransactionNumber,LaneNumber,PaymentTransactionId 
        ,DateTime,IsPaid,FineAmount,PaymentMethod,ReceiptNumber,BillNumber 
        ,WarehouseCharge,DriversLicenseNumber,TransportAgencyInformation 
        FROM FinePayment
        WHERE StationId IN @StationIds";

        if(!string.IsNullOrEmpty(laneNumbers))
        {
            query += " AND LaneNumber IN (" + laneNumbers + ")";
        }
        query += @" AND DATEDIFF(Day,DateTime,@DateStart)<=0
            AND DATEDIFF(Day,DateTime,@DateEnd)>=0";

        var parameters = new
        {
            StationIds = reportParameters.Stations,
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd
        };

        try
        {
            IEnumerable<FinePayment> reports = await _db.LoadData<FinePayment, object>(query, parameters);
            isSuccess = true;
            return (reports, isSuccess, message);
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (null, isSuccess, message);
    }

    public async Task<(bool,string)> Add(FinePayment obj)
    {
        bool isSuccess = false;
        string message = "";
        string query = @"INSERT INTO FinePayment(StationId,LaneNumber,TransactionNumber,PaymentTransactionId,DateTime,IsPaid,FineAmount,PaymentMethod,ReceiptNumber,BillNumber,WarehouseCharge,DriversLicenseNumber,TransportAgencyInformation)
                        VALUES(@StationId,@LaneNumber,@TransactionNumber,@PaymentTransactionId,@DateTime,@IsPaid,@FineAmount,@PaymentMethod,@ReceiptNumber,@BillNumber,@WarehouseCharge,@DriversLicenseNumber,@TransportAgencyInformation)";
        try
        { 
            isSuccess = await _db.SaveData(query, obj);
        }
        catch (SqlException ex)
        {
            //Duplicate error
            if (ex.Number == 2627)
            {
                isSuccess = false;
                message = "Duplicate data";
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (isSuccess, message);
    }
    public async Task<(bool,string)> Add(List<FinePayment> obj)
    {
        bool isSuccess = false;
        string message = "";
        string query = @"INSERT INTO FinePayment(StationId,LaneNumber,TransactionNumber,PaymentTransactionId,DateTime,IsPaid,FineAmount,PaymentMethod,ReceiptNumber,BillNumber,WarehouseCharge,DriversLicenseNumber,TransportAgencyInformation)
                        VALUES(@StationId,@LaneNumber,@TransactionNumber,@PaymentTransactionId,@DateTime,@IsPaid,@FineAmount,@PaymentMethod,@ReceiptNumber,@BillNumber,@WarehouseCharge,@DriversLicenseNumber,@TransportAgencyInformation)";
        try
        {
            isSuccess = await _db.SaveData(query, obj);
        }
        catch (SqlException ex)
        {
            //Duplicate error
            if (ex.Number == 2627)
            {
                isSuccess = false;
                message = "Duplicate data";
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (isSuccess, message);  
    }

    public async Task<bool> Delete(UploadedFile file)
    {
        string query = @"DELETE FROM FinePaymentProcess WHERE FileId=@FileId
            DELETE FROM FinePayment WHERE StationId=@StationId AND DATEDIFF(DAY,DateTime,@Date)=0";

        return await _db.SaveData(query, new { FileId=file.Id, file.StationId, file.Date });
    }
}

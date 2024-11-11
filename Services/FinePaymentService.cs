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

        string stationIds = string.Join(",", reportParameters.Stations);
        string laneNumbers = string.Join(",", reportParameters.WIMScales.Select(ws => ws.LaneNumber));

        string query = @"SELECT FP.TransactionNumber, FP.LaneNumber, FP.PaymentTransactionId, FP.DateTime, FP.IsPaid,
                            FP.FineAmount, FP.PaymentMethod, FP.ReceiptNumber, FP.BillNumber,
                            FP.WarehouseCharge, FP.DriversLicenseNumber, FP.TransportAgencyInformation
                     FROM FinePayment FP
                     WHERE FP.TransactionNumber IN (
                        SELECT AL.TransactionNumber 
                        FROM AxleLoad AL";

        query += this.GetFilterClause(reportParameters);

        query += @" AND AL.StationId IN @StationIds)";

        var parameters = new
        {
            StationIds = reportParameters.Stations,
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
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
    private string GetFilterClause(ReportParameters reportParameters)
    {
        string query = @" WHERE DATEDIFF(Day, AL.DateTime, @DateStart) <= 0
            AND DATEDIFF(Day, AL.DateTime, @DateEnd) >= 0";
        if (reportParameters.WIMScales is not null && reportParameters.WIMScales.Any())
        {
            if (reportParameters.WIMScales.Count() == 1)
            {
                query += " AND AL.LaneNumber = " + reportParameters.WIMScales.FirstOrDefault().LaneNumber;
            }
            else
            {
                query += " AND AL.LaneNumber IN (" + string.Join(",", reportParameters.WIMScales.Select(ws => "(" + ws.LaneNumber + ")")) + ")";
            }
        }
        if (reportParameters.NumberOfAxle is not null && reportParameters.NumberOfAxle.Any())
        {
            if (reportParameters.NumberOfAxle.Count() == 1)
            {
                query += " AND AL.NumberOfAxle = " + reportParameters.NumberOfAxle.FirstOrDefault();
            }
            else
            {
                query += " AND AL.NumberOfAxle IN (" + string.Join(",", reportParameters.NumberOfAxle.Select(na => "(" + na + ")")) + ")";
            }
        }
        if (reportParameters.Wheelbase > 0)
        {
            query += " AND Wheelbase = @Wheelbase";
        }
        if (reportParameters.ClassStatus > 0)
        {
            query += " AND ClassStatus = @ClassStatus";
        }
        return query;
    }
}

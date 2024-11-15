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

        string query = @"SELECT SN.StationName,FP.StationId,FP.TransactionNumber,FP.LaneNumber,FP.PaymentTransactionId,FP.DateTime,FP.IsPaid,FP.FineAmount,FP.PaymentMethod,FP.ReceiptNumber,FP.BillNumber,FP.WarehouseCharge,FP.DriversLicenseNumber,FP.TransportAgencyInformation,FP.EntryTime
                FROM FinePayment FP INNER JOIN Stations SN ON FP.StationId=SN.StationId ";

        query += this.GetFilterClause(reportParameters);

        var parameters = new
        {
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
    private string GetFilterClauseOld(ReportParameters reportParameters)
    {
        string query = @" WHERE DATEDIFF(Day, FP.DateTime, @DateStart) <= 0
            AND DATEDIFF(Day, FP.DateTime, @DateEnd) >= 0";
        if (reportParameters.WIMScales is not null && reportParameters.WIMScales.Any())
        {
            if (reportParameters.WIMScales.Count() == 1)
            {
                query += " AND FP.LaneNumber = " + reportParameters.WIMScales.FirstOrDefault().LaneNumber;
            }
            else
            {
                query += " AND FP.LaneNumber IN (" + string.Join(",", reportParameters.WIMScales.Select(ws => "(" + ws.LaneNumber + ")")) + ")";
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

    private string GetStationTableQuery(ReportParameters reportParameters)
    {
        if (reportParameters.Stations.Count == 1)
        {
            return "";
        }

        string query;

        if (reportParameters.WIMType > 0 || reportParameters.UpboundDirection || reportParameters.DownboundDirection)
        {
            string stationIds = string.Join(",", reportParameters.Stations.Select(s => s));
            query = @" DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT,LaneNo INT)
            INSERT INTO @Stations(StationId,LaneNo)
            SELECT StationId,LaneNumber
            FROM WIMScale
            WHERE StationId IN (" + stationIds + ")";
            if (reportParameters.WIMType > 0)
            {
                query += " AND Type=" + reportParameters.WIMType;
            }
            else if (reportParameters.UpboundDirection)
            {
                query += " AND IsUpbound=1";
            }
            else if (reportParameters.DownboundDirection)
            {
                query += " AND IsUpbound=0";
            }

            return query;
        }
        else
        {
            string stationIds = string.Join(",", reportParameters.Stations.Select(s => "(" + s + ")"));
            query = @" DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT)
            INSERT INTO @Stations(StationId) VALUES " + stationIds + " ";
        }

        return query;
    }
    private string GetFilterClause(ReportParameters reportParameters)
    {
        bool needAxleLoadJoining = false;
        string joining = String.Empty;
        string whereClause = @" WHERE DATEDIFF(Day, FP.DateTime, @DateStart) <= 0
            AND DATEDIFF(Day, FP.DateTime, @DateEnd) >= 0";

        if (reportParameters.Stations.Count() == 1)
        {
            whereClause += " AND FP.StationId=" + reportParameters.Stations.FirstOrDefault();
        }
        else
        {
            if (reportParameters.WIMType > 0 || reportParameters.UpboundDirection || reportParameters.DownboundDirection)
            {
                joining = " INNER JOIN @Stations S ON FP.StationId = S.StationId AND FP.LaneNumber=S.LaneNo ";
            }
            else
            {
                joining = " INNER JOIN @Stations S ON FP.StationId = S.StationId ";
            }
        }
        if (reportParameters.TimeStart != reportParameters.TimeEnd)
        {
            whereClause += " AND CAST(FP.DateTime AS TIME) >= @TimeStart AND CAST(FP.DateTime AS TIME) <=@TimeEnd";
        }
        if (reportParameters.WIMScales is not null && reportParameters.WIMScales.Any())
        {
            if (reportParameters.WIMScales.Count() == 1)
            {
                whereClause += " AND FP.LaneNumber = " + reportParameters.WIMScales.FirstOrDefault().LaneNumber;
            }
            else
            {
                whereClause += " AND FP.LaneNumber IN (" + string.Join(",", reportParameters.WIMScales.Select(ws => "(" + ws.LaneNumber + ")")) + ")";
            }
        }
        if (reportParameters.NumberOfAxle is not null && reportParameters.NumberOfAxle.Any())
        {
            needAxleLoadJoining = true;
            if (reportParameters.NumberOfAxle.Count() == 1)
            {
                whereClause += " AND AL.NumberOfAxle = " + reportParameters.NumberOfAxle.FirstOrDefault();
            }
            else
            {
                whereClause += " AND AL.NumberOfAxle IN (" + string.Join(",", reportParameters.NumberOfAxle.Select(na => "(" + na + ")")) + ")";
            }
        }
        if (!string.IsNullOrEmpty(reportParameters.WeightFilterColumn))
        {
            needAxleLoadJoining = true;
            whereClause += " AND (AL." + reportParameters.WeightFilterColumn + ">=" + reportParameters.WeightMin + " AND AL." + reportParameters.WeightFilterColumn + "<=" + reportParameters.WeightMax + ")";
        }
        if (reportParameters.Wheelbase > 0)
        {
            needAxleLoadJoining = true;
            whereClause += " AND AL.Wheelbase = @Wheelbase";
        }
        if (reportParameters.ClassStatus > 0)
        {
            needAxleLoadJoining = true;
            whereClause += " AND AL.ClassStatus = @ClassStatus";
        }

        if (needAxleLoadJoining)
        {
            joining += " INNER JOIN AxleLoad AL ON FP.StationId=AL.StationId AND FP.LaneNumber=AL.LaneNumber AND FP.TransactionNumber=AL.TransactionNumber ";
        }

        if (!string.IsNullOrEmpty(joining))
        {
            return joining + whereClause;
        }

        return whereClause;
    }
}

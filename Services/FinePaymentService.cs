using BOL;
using Services.Helpers;
using System.Data.SqlClient;

namespace Services;

public interface IFinePaymentService
{
    Task<IEnumerable<FinePayment>> Get(FinePayment obj);
    Task<(bool,string)> Add(FinePayment obj);
    Task<(bool,string)> Add(List<FinePayment> obj);
    Task<bool> Delete(UploadedFile file);
}

public class FinePaymentService(ISqlDataAccess _db) : IFinePaymentService
{
    public async Task<IEnumerable<FinePayment>> Get(FinePayment obj)
    {
        string query = @"SELECT * FROM FinePayment WHERE StationId=@StationId AND DATEDIFF(DAY,DateTime,@DateTime)=0";

        return await _db.LoadData<FinePayment, object>(query, obj);
    }
    public async Task<(bool,string)> Add(FinePayment obj)
    {
        bool isSuccess = false;
        string message = "";
        string query = @"INSERT INTO FinePayment(StationId,TransactionNumber,PaymentTransactionId,DateTime,IsPaid,FineAmount,PaymentMethod,ReceiptNumber,BillNumber,WarehouseCharge,DriversLicenseNumber,TransportAgencyInformation)
                        VALUES(@StationId,@TransactionNumber,@PaymentTransactionId,@DateTime,@IsPaid,@FineAmount,@PaymentMethod,@ReceiptNumber,@BillNumber,@WarehouseCharge,@DriversLicenseNumber,@TransportAgencyInformation)";
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
        string query = @"INSERT INTO FinePayment(StationId,TransactionNumber,PaymentTransactionId,DateTime,IsPaid,FineAmount,PaymentMethod,ReceiptNumber,BillNumber,WarehouseCharge,DriversLicenseNumber,TransportAgencyInformation)
                        VALUES(@StationId,@TransactionNumber,@PaymentTransactionId,@DateTime,@IsPaid,@FineAmount,@PaymentMethod,@ReceiptNumber,@BillNumber,@WarehouseCharge,@DriversLicenseNumber,@TransportAgencyInformation)";
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

using BOL;
using Services.Helpers;

namespace Services;

public interface IFinePaymentService
{
    Task<bool> Add(FinePayment obj);
    Task<bool> Add(List<FinePayment> obj);
    Task<bool> Delete(DateTime date);
}

public class FinePaymentService(ISqlDataAccess _db) : IFinePaymentService
{
    public async Task<bool> Add(FinePayment obj)
    {
        string query = @"INSERT INTO FinePayment(StationId,TransactionNumber,DateTime,IsPaid,FineAmount,PaymentMethod,ReceiptNumber,BillNumber,WarehouseCharge,DriversLicenseNumber)
                        VALUES(@StationId,@TransactionNumber,@DateTime,@IsPaid,@FineAmount,@PaymentMethod,@ReceiptNumber,@BillNumber,@WarehouseCharge,@DriversLicenseNumber)";

        return await _db.SaveData(query, obj);
    }
    public async Task<bool> Add(List<FinePayment> obj)
    {
        string query = @"INSERT INTO FinePayment(StationId,TransactionNumber,DateTime,IsPaid,FineAmount,PaymentMethod,ReceiptNumber,BillNumber,WarehouseCharge,DriversLicenseNumber)
                        VALUES(@StationId,@TransactionNumber,@DateTime,@IsPaid,@FineAmount,@PaymentMethod,@ReceiptNumber,@BillNumber,@WarehouseCharge,@DriversLicenseNumber)";

        return await _db.SaveData(query, obj);
    }

    public async Task<bool> Delete(DateTime date)
    {
        string query = "DELETE FROM FinePayment WHERE DATEDIFF(DAY,DateTime,@Date)=0";

        return await _db.SaveData(query, new { Date = date });
    }
}

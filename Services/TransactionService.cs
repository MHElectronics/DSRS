using BOL;
using Services.Helpers;
namespace Services;
public interface ITransactionService
{
    Task<IEnumerable<Transaction>> GetTransactionList();
    Task<Transaction> GetTransactionById(int id);
    Task<bool> InsertTransaction(Transaction Transaction, User user);
    Task<Transaction> UpdateTransaction(Transaction Transaction, User user);
    Task<bool> DeleteTransaction(int id, User user);

}
public class TransactionService : ITransactionService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;

    public TransactionService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
    }
    public async Task<Transaction> GetTransactionById(int id)
    {
        IEnumerable<Transaction>? results = await _db.LoadData<Transaction, dynamic>("SELECT * FROM Transaction WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }
    public async Task<IEnumerable<Transaction>> GetTransactionList() =>
        await _db.LoadData<Transaction, dynamic>("SELECT * FROM Transaction", new { });

    public async Task<bool> InsertTransaction(Transaction Transaction, User user)
    {
        bool hasDuplicate = await this.CheckDuplicateTransaction(Transaction);
        if (!hasDuplicate)
        {
            string sql = @"INSERT INTO Transaction(CustomerId,CustomerName,TransactionId,TransactionTime,Type,Narration,IsSystemGenerated,Status,MeterNumber,Amount) VALUES(@CustomerId,@CustomerName,@TransactionId,@TransactionTime,@Type,@Narration,@IsSystemGenerated,@Status,@MeterNumber,@Amount)";
            bool isSuccess = await _db.SaveData<Transaction>(sql, Transaction);

            if (isSuccess)
            {
                UserActivity log = new UserActivity(user.Id, Transaction.Id + "Transaction Added", LogActivity.Insert);
                await _userActivityService.InsertUserActivity(log);
            }

            return isSuccess;
        }
        return false;
    }

    public async Task<Transaction> UpdateTransaction(Transaction Transaction, User user)
    {
        string sql = @"UPDATE Transaction SET CustomerId=@CustomerId, CustomerName=@CustomerName, TransactionId=@TransactionId, TransactionTime=@TransactionTime, Type=@Type, Narration=@Narration, IsSystemGenerated=@IsSystemGenerated, Status=@Status, MeterNumber=@MeterNumber, Amount=@Amount   WHERE Id=@Id";
        await _db.SaveData(sql, Transaction);

        UserActivity log = new UserActivity(user.Id, Transaction.Id + "Transaction Updated", LogActivity.Update);
        await _userActivityService.InsertUserActivity(log);

        return Transaction;
    }
    public async Task<bool> DeleteTransaction(int id, User user)
    {
        string query = "DELETE FROM Transaction WHERE Id=@Id";
        int count = await _db.DeleteData<Transaction, object>(query, new { id });

        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, "Transaction Delete", LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }
        return count > 0;
    }


    private async Task<bool> CheckDuplicateTransaction(Transaction Transaction)
    {
        string query = "SELECT COUNT(1) Count FROM Transaction WHERE(LOWER(TransactionId)=LOWER(@TransactionId) OR Id=@Id)";
        return await _db.LoadSingleAsync<bool, object>(query, Transaction);
    }
}

using BOL;
using Services.Helpers;
namespace Services;
public interface ICustomerService
{
    Task<IEnumerable<Customer>> GetcustomerList();
    Task<Customer> GetcustomerById(int id);
    Task<bool> Insertcustomer(Customer customer, User user);
    Task<Customer> Updatecustomer(Customer customer, User user);
    Task<bool> Deletecustomer(int id, User user);

}
public class CustomerService : ICustomerService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;

    public CustomerService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
    }
    public async Task<Customer> GetcustomerById(int id)
    {
        IEnumerable<Customer>? results = await _db.LoadData<Customer, dynamic>("SELECT * FROM Customer WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }
    public async Task<IEnumerable<Customer>> GetcustomerList() =>
        await _db.LoadData<Customer, dynamic>("SELECT * FROM Customer", new { });

    public async Task<bool> Insertcustomer(Customer customer, User user)
    {
        bool hasDuplicate = await this.CheckDuplicatecustomer(customer);
        if (!hasDuplicate)
        {
            string sql = @"INSERT INTO customer(Name,Description,Address,ContactNo,MeterType) VALUES(@Name,@Description,@Address,@ContactNo,@MeterType)";
            bool isSuccess = await _db.SaveData<Customer>(sql, customer);

            if (isSuccess)
            {
                UserActivity log = new UserActivity(user.Id, customer.Name + "customer Added", LogActivity.Insert);
                await _userActivityService.InsertUserActivity(log);
            }

            return isSuccess;
        }
        return false;
    }

    public async Task<Customer> Updatecustomer(Customer customer, User user)
    {
        string sql = @"UPDATE customer SET Name=@Name, Description=@Description, Address=@Address, ContactNo=@ContactNo, MeterType=@MeterType  WHERE Id=@Id";
        await _db.SaveData(sql, customer);

        UserActivity log = new UserActivity(user.Id, customer.Name + "customer Updated", LogActivity.Update);
        await _userActivityService.InsertUserActivity(log);

        return customer;
    }
    public async Task<bool> Deletecustomer(int id, User user)
    {
        string query = "DELETE FROM customer WHERE Id=@Id";
        int count = await _db.DeleteData<Customer, object>(query, new { id });

        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, "customer Delete", LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }
        return count > 0;
    }


    private async Task<bool> CheckDuplicatecustomer(Customer customer)
    {
        string query = "SELECT COUNT(1) Count FROM customer WHERE(LOWER(Name)=LOWER(@Name) OR Id=@Id)";
        return await _db.LoadSingleAsync<bool, object>(query, customer);
    }
}

using BOL;
using Services.Helpers;
namespace Services;
public interface ICompanyService
{
    Task<IEnumerable<Company>> GetCompanyList();
    Task<Company> GetCompanyById(int id);
    Task<bool> InsertCompany(Company company, User user);
    Task<Company> UpdateCompany(Company company, User user);
    Task<bool> DeleteCompany(int id, User user);

}
public  class CompanyService: ICompanyService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;

    public CompanyService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
    }
    public async Task<Company> GetCompanyById(int id)
    {
        IEnumerable<Company>? results = await _db.LoadData<Company, dynamic>("SELECT * FROM Company WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }
    public async Task<IEnumerable<Company>> GetCompanyList() =>
        await _db.LoadData<Company, dynamic>("SELECT * FROM Company", new { });

    public async Task<bool> InsertCompany(Company company, User user)
    {
        bool hasDuplicate = await this.CheckDuplicateCompany(company);
        if (!hasDuplicate)
        {
            string sql = @"INSERT INTO Company(Name,Description,Address,ContactNo,MeterType) VALUES(@Name,@Description,@Address,@ContactNo,@MeterType)";
            bool isSuccess = await _db.SaveData<Company>(sql, company);

            if (isSuccess)
            {
                UserActivity log = new UserActivity(user.Id, company.Name + "Company Added", LogActivity.Insert);
                await _userActivityService.InsertUserActivity(log);
            }

            return isSuccess;
        }
        return false;
    }

    public async Task<Company> UpdateCompany(Company company, User user)
    {
        string sql = @"UPDATE Company SET Name=@Name, Description=@Description, Address=@Address, ContactNo=@ContactNo, MeterType=@MeterType  WHERE Id=@Id";
        await _db.SaveData(sql, company);

        UserActivity log = new UserActivity(user.Id, company.Name + "Company Updated", LogActivity.Update);
        await _userActivityService.InsertUserActivity(log);

        return company;
    }
    public async Task<bool> DeleteCompany(int id, User user)
    {
        string query = "DELETE FROM Company WHERE Id=@Id";
        int count = await _db.DeleteData<Company, object>(query, new { id });

        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, "Company Delete", LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }
        return count > 0;
    }


    private async Task<bool> CheckDuplicateCompany(Company company)
    {
        string query = "SELECT COUNT(1) Count FROM Company WHERE(LOWER(Name)=LOWER(@Name) OR Id=@Id)";
        return await _db.LoadSingleAsync<bool, object>(query, company);
    }
}

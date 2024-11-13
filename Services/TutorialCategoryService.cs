using BOL;
using DocumentFormat.OpenXml.Spreadsheet;
using Services.Helpers;

namespace Services;
public interface ICategoryService
{
    Task<IEnumerable<TutorialCategory>> GetCategoryList();
    Task<TutorialCategory> GetCategory(int id);
    Task<bool> InsertCategory(TutorialCategory category, User user);
    Task<TutorialCategory> UpdateCategory(TutorialCategory category, User user);
    Task<bool> DeleteCategory(TutorialCategory category, User user);
}
public class TutorialCategoryService : ICategoryService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;
    public TutorialCategoryService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
    }

    public async Task<TutorialCategory> GetCategory(int id)
    {
        var results = await _db.LoadData<TutorialCategory, dynamic>("SELECT * FROM TutorialCategory WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }
    public async Task<IEnumerable<TutorialCategory>> GetCategoryList() =>
        await _db.LoadData<TutorialCategory, dynamic>("SELECT * FROM TutorialCategory", new { });

    public async Task<bool> InsertCategory(TutorialCategory category, User user)
    {
        string sql = @"INSERT INTO TutorialCategory(Name) VALUES(@Name)";
        bool isSuccess = await _db.SaveData<TutorialCategory>(sql, category);

        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "Tutorail Category Added: " + category.Name, LogActivity.Insert);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }

    public async Task<TutorialCategory> UpdateCategory(TutorialCategory category, User user)
    {
        string sql = @"UPDATE TutorialCategory SET Name=@Name WHERE Id=@Id";
        await _db.SaveData(sql, category);

        UserActivity log = new UserActivity(user.Id, "Tutorail Category Updated: " + category.Id, LogActivity.Update);
        await _userActivityService.InsertUserActivity(log);

        return category;
    }
    public async Task<bool> DeleteCategory(TutorialCategory category, User user)
    {
        string query = "DELETE FROM TutorialCategory WHERE Id=@Id";
        int count = await _db.DeleteData<TutorialCategory, object>(query, new { category.Id });

        if(count > 0)
        {
            UserActivity log = new UserActivity(user.Id, "Tutorail Category Deleted: " + category.Name, LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }

        return count > 0;
    }
}

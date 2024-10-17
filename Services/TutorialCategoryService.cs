using BOL;
using Services.Helpers;

namespace Services;
public interface ICategoryService
{
    Task<IEnumerable<TutorialCategory>> GetCategoryList();
    Task<TutorialCategory> GetCategory(int id);
    Task<bool> InsertCategory(TutorialCategory category);
    Task<TutorialCategory> UpdateCategory(TutorialCategory category);
    Task<bool> DeleteCategory(int id);
}
public class TutorialCategoryService : ICategoryService
{
    private readonly ISqlDataAccess _db;
    public TutorialCategoryService(ISqlDataAccess db)
    {
        _db = db;
    }

    public async Task<bool> DeleteCategory(int id)
    {
        string query = "DELETE FROM TutorialCategory WHERE Id=@Id";
        int count = await _db.DeleteData<TutorialCategory, object>(query, new { id });
        return count > 0;
    }

    public async Task<TutorialCategory> GetCategory(int id)
    {
        var results = await _db.LoadData<TutorialCategory, dynamic>("SELECT * FROM TutorialCategory WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<TutorialCategory>> GetCategoryList() =>
        await _db.LoadData<TutorialCategory, dynamic>("SELECT * FROM TutorialCategory", new { });

    public async Task<bool> InsertCategory(TutorialCategory Category)
    {
        string sql = @"INSERT INTO TutorialCategory(Name) VALUES(@Name)";
        return await _db.SaveData<TutorialCategory>(sql, Category);   
    }

    public async Task<TutorialCategory> UpdateCategory(TutorialCategory Category)
    {
        string sql = @"UPDATE TutorialCategory SET Name=@Name WHERE Id=@Id";
        await _db.SaveData(sql, Category);
        return Category;
    }
}

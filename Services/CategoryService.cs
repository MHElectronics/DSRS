using BOL;
using Services.Helpers;

namespace Services;
public interface ICategoryService
{
    Task<IEnumerable<Category>> GetCategoryList();
    Task<Category> GetCategory(int id);
    Task<bool> InsertCategory(Category category);
    Task<Category> UpdateCategory(Category category);
    Task<bool> DeleteCategory(int id);
}
public class CategoryService : ICategoryService
{
    private readonly ISqlDataAccess _db;
    public CategoryService(ISqlDataAccess db)
    {
        _db = db;
    }

    public async Task<bool> DeleteCategory(int id)
    {
        string query = "DELETE FROM Category WHERE Id=@Id";
        int count = await _db.DeleteData<Category, object>(query, new { id });
        return count > 0;
    }

    public async Task<Category> GetCategory(int id)
    {
        var results = await _db.LoadData<Category, dynamic>("SELECT * FROM Category WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<Category>> GetCategoryList() =>
        await _db.LoadData<Category, dynamic>("SELECT * FROM Category", new { });

    public async Task<bool> InsertCategory(Category Category)
    {
        string sql = @"INSERT INTO Category(Name) VALUES(@Name)";
        return await _db.SaveData<Category>(sql, Category);   
    }

    public async Task<Category> UpdateCategory(Category Category)
    {
        string sql = @"UPDATE Category SET Name=@Name WHERE Id=@Id";
        await _db.SaveData(sql, Category);
        return Category;
    }
}

using BOL;
using Services.Helpers;

namespace Services;

public interface IFAQService
{
    Task<IEnumerable<FAQ>> GetFAQs(bool onlyPublished = false);
    Task<IEnumerable<FAQ>> GetByUser(User user);
    Task<bool> InsertFAQ(FAQ faq);
    Task<bool> UpdateQuestion(FAQ faq);
}

public class FAQService : IFAQService
{
    private readonly ISqlDataAccess _db;

    public FAQService(ISqlDataAccess db)
    {
        _db = db;
    }

    public async Task<IEnumerable<FAQ>> GetFAQs(bool onlyPublished = false)
    {
        string sql = "SELECT * FROM FAQ";
        Dictionary<string, object> param = new Dictionary<string, object>();

        if (onlyPublished)
        {
            sql += " WHERE IsPublished=@IsPublished";
            param.Add("@IsPublished", true);
        }

        return await _db.LoadData<FAQ, dynamic>(sql, param);
    }
    public async Task<IEnumerable<FAQ>> GetByUser(User user)
    {
        string sql = "SELECT * FROM FAQ WHERE QuestionUserId=@UserId";
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { "@UserId", user.Id }
        };

        return await _db.LoadData<FAQ, dynamic>(sql, param);
    }

    public async Task<bool> InsertFAQ(FAQ faq)
    {
        string sql = @"INSERT INTO FAQ(Question,Answer,QuestionUserId,AnswerUserId,IsPublished,DisplayOrder)
            VALUES (@Question,@Answer,@QuestionUserId,@AnswerUserId,@IsPublished,@DisplayOrder)";
        return await _db.SaveData<FAQ>(sql, faq);
    }
    public async Task<bool> UpdateQuestion(FAQ faq)
    {
        string sql = @"UPDATE FAQ SET Question=@Question WHERE Id=@Id AND (AnswerUserId=0 OR AnswerUserId IS NULL)";
        return await _db.SaveData<FAQ>(sql, faq);
    }
}

using BOL;
using DocumentFormat.OpenXml.Spreadsheet;
using Services.Helpers;

namespace Services;

public interface IFAQService
{
    Task<IEnumerable<FAQ>> GetFAQs(bool onlyPublished = false);
    Task<IEnumerable<FAQ>> GetByUser(User user);
    Task<int> GetUnansweredFAQCount();

    Task<bool> InsertFAQ(FAQ faq, User user);
    Task<bool> UpdateFAQ(FAQ faq, User user);
    Task<bool> DeleteFAQ(FAQ faq, User user);
    Task<bool> UpdateQuestion(FAQ faq, User user);
}

public class FAQService : IFAQService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;
    public FAQService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
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

    public async Task<int> GetUnansweredFAQCount()
    {
        string sql = "SELECT COUNT(1) FROM FAQ WHERE AnswerUserId=0";
        return await _db.LoadSingleAsync<int, dynamic>(sql, null);
    }

    public async Task<bool> InsertFAQ(FAQ faq, User user)
    {
        faq.EntryTime = DateTime.Now;
        string sql = @"INSERT INTO FAQ(Question,Answer,QuestionUserId,AnswerUserId,IsPublished,DisplayOrder,EntryTime)
            VALUES (@Question,@Answer,@QuestionUserId,@AnswerUserId,@IsPublished,@DisplayOrder,@EntryTime)";
        bool isSuccess = await _db.SaveData<FAQ>(sql, faq);

        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "FAQ Added", LogActivity.Insert);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }
    public async Task<bool> UpdateFAQ(FAQ faq, User user)
    {
        string sql = @"UPDATE FAQ SET Question=@Question, Answer=@Answer, QuestionUserId=@QuestionUserId, AnswerUserId=@AnswerUserId, IsPublished=@IsPublished, DisplayOrder=@DisplayOrder
                       WHERE Id=@Id";
        bool isSuccess = await _db.SaveData<FAQ>(sql, faq);

        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "FAQ Updated. FAQ Id: " + faq.Id , LogActivity.Update);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }
    public async Task<bool> UpdateQuestion(FAQ faq, User user)
    {
        string sql = @"UPDATE FAQ SET Question=@Question, QuestionUserId=@QuestionUserId WHERE Id=@Id AND (AnswerUserId=0 OR AnswerUserId IS NULL)";
        bool isSuccess = await _db.SaveData<FAQ>(sql, faq);

        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "FAQ Question Updated. FAQ Id: " + faq.Id, LogActivity.Update);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }
    public async Task<bool> DeleteFAQ(FAQ faq, User user)
    {
        string sql = "DELETE FROM FAQ WHERE Id=@Id";
        int count = await _db.DeleteData<FAQ, object>(sql, new { faq.Id });
        
        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, "FAQ Deleted. FAQ Id: " + faq.Id, LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }

        return count > 0;
    }
}

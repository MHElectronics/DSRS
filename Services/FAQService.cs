using BOL;
using Services.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services;

public interface IFAQService
{
    Task<IEnumerable<FAQ>> GetFAQs(bool onlyPublished = false);
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
        //return [new FAQ() { Id = 1, Question = "Question 1", Answer = "Answer 1" }
        //        ,new FAQ() { Id = 2, Question = "Question 2", Answer = "Answer 2" }];

        string sql = "SELECT * FROM FAQ";
        Dictionary<string, object> param = new Dictionary<string, object>();

        if (onlyPublished)
        {
            sql += " WHERE IsPublished=@IsPublished";
            param.Add("@IsPublished", true);
        }
        
        return await _db.LoadData<FAQ, dynamic>(sql, param);
    }
}

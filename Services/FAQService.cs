using BOL;
using Services.Helpers;

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
        return [new FAQ() { Id = 1, Question = "Question 1", Answer = "Answer 1" }
                ,new FAQ() { Id = 2, Question = "Question 2", Answer = "Answer 2" }];
    }
}

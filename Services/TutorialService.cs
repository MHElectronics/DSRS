using BOL;
using BOL.Models;
using Services.Helpers;

namespace Services;
public interface ITutorialService
{
    Task<IEnumerable<Tutorial>> GetTutorials();
    Task<IEnumerable<Tutorial>> GetByUser(User user);
    Task<int> InsertTutorial(Tutorial Tutorial, User user);
    Task<bool> UpdateTutorial(Tutorial Tutorial, User user);
    Task<bool> DeleteTutorial(Tutorial Tutorial, User user);
}
public class TutorialService : ITutorialService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;
    public TutorialService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
    }
    
    public async Task<IEnumerable<Tutorial>> GetTutorials() =>
        await _db.LoadData<Tutorial, dynamic>("SELECT * FROM Tutorial", new { });
    public async Task<IEnumerable<Tutorial>> GetByUser(User user)
    {
        string sql = "SELECT * FROM Tutorial WHERE UserId=@UserId";
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { "@UserId", user.Id }
        };

        return await _db.LoadData<Tutorial, dynamic>(sql, param);
    }

    public async Task<int> InsertTutorial(Tutorial tutorial, User user)
    {
        string sql = @"INSERT INTO Tutorial(FileName,FileLocation,Description,UserId,DisplayOrder,Date,TutorialCategoryId)
                        OUTPUT INSERTED.Id
                        VALUES (@FileName,@FileLocation,@Description,@UserId,@DisplayOrder,@Date,@TutorialCategoryId)";
        int TutorialId = await _db.ExecuteScalar<int>(sql, tutorial);

        if(TutorialId > 0)
        {
            UserActivity log = new UserActivity(user.Id, "Tutorial Added", LogActivity.Insert);
            await _userActivityService.InsertUserActivity(log);
        }

        return TutorialId;
    }
    public async Task<bool> UpdateTutorial(Tutorial tutorial, User user)
    {
        string sql = @"UPDATE Tutorial SET FileName=@FileName, FileLocation=@FileLocation, Description=@Description, UserId=@UserId,DisplayOrder=@DisplayOrder, Date=@Date, TutorialCategoryId=@TutorialCategoryId
                       WHERE Id=@Id";
        bool isSuccess = await _db.SaveData<Tutorial>(sql, tutorial);

        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "Tutorial updated: " + tutorial.Id, LogActivity.Update);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }
    public async Task<bool> DeleteTutorial(Tutorial tutorial, User user)
    {
        string sql = "DELETE FROM Tutorial WHERE Id=@Id";
        int count = await _db.DeleteData<Tutorial, object>(sql, new { tutorial.Id });

        if (count > 0)
        {
            UserActivity log = new UserActivity(user.Id, "Tutorial Deleted: " + tutorial.Id, LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }

        return count > 0;
    }
}

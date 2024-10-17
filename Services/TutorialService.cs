using BOL;
using BOL.Models;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services;
public interface ITutorialService
{
    Task<IEnumerable<Tutorial>> GetTutorials();
    Task<IEnumerable<Tutorial>> GetByUser(User user);
    Task<int> InsertTutorial(Tutorial Tutorial);
    Task<bool> UpdateTutorial(Tutorial Tutorial);
    Task<bool> DeleteTutorial(Tutorial Tutorial);
}
public class TutorialService : ITutorialService
{
    private readonly ISqlDataAccess _db;

    public TutorialService(ISqlDataAccess db)
    {
        _db = db;
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

    public async Task<int> InsertTutorial(Tutorial Tutorial)
    {
        string sql = @"INSERT INTO Tutorial(FileName,FileLocation,Description,UserId,DisplayOrder,Date,TutorialCategoryId)
                        OUTPUT INSERTED.Id
                        VALUES (@FileName,@FileLocation,@Description,@UserId,@DisplayOrder,@Date,@TutorialCategoryId)";
        int TutorialId = await _db.ExecuteScalar<int>(sql, Tutorial);
        return TutorialId;
    }

    public async Task<bool> UpdateTutorial(Tutorial Tutorial)
    {
        string sql = @"UPDATE Tutorial SET FileName=@FileName, FileLocation=@FileLocation, Description=@Description, UserId=@UserId,DisplayOrder=@DisplayOrder, Date=@Date, TutorialCategoryId=@TutorialCategoryId
                       WHERE Id=@Id";
        return await _db.SaveData<Tutorial>(sql, Tutorial);
    }
    public async Task<bool> DeleteTutorial(Tutorial Tutorial)
    {
        string sql = "DELETE FROM Tutorial WHERE Id=@Id";
        int count = await _db.DeleteData<Tutorial, object>(sql, new { Tutorial.Id });
        return count > 0;
    }
}

using BOL;
using DocumentFormat.OpenXml.Spreadsheet;
using Services.Helpers;

namespace Services;

public interface IUserService
{
    Task<bool> AuthenticateUser(User user);
    Task<User?> GetUser(User user);
    Task<IEnumerable<User>> GetUsers();
    Task<User?> GetUserById(int id);
    Task<User?> GetByEmail(User user);
    Task<User> InsertUser(User user, User activityUser);
    Task<User> UpdateUser(User user, User activityUser);
    Task<bool> Delete(User user, User activityUser);
    Task<bool> ChangePassword(User user, User activityUser);
}

public class UserService : IUserService
{
    private readonly ISqlDataAccess _db;
    private readonly IUserActivityService _userActivityService;
    public UserService(ISqlDataAccess db, IUserActivityService userActivityService)
    {
        _db = db;
        _userActivityService = userActivityService;
    }

    public async Task<bool> AuthenticateUser(User user)
    {
        User? checkUser = await GetByEmail(user);
        if (checkUser is not null)
        {
            user.Password = new SecurityHelper().CreatePasswordHash(user.Password, checkUser.PasswordSalt);
            var results = await _db.LoadData<User, dynamic>("SELECT * FROM Users WHERE Email=@Email AND Password=@Password", new { Email = user.Email, Password = user.Password });
            return results.Any();
        }
        return false;
    }
    public async Task<IEnumerable<User>> GetUsers() =>
        await _db.LoadData<User, dynamic>("SELECT * FROM Users", new { });


    public async Task<User?> GetUser(User user)
    {
        user = await _db.LoadSingleAsync<User, dynamic>("SELECT * FROM Users WHERE Id=@Id OR Email=@Email", new { Id = user.Id, Email = user.Email });
        user.Password = "";
        return user;
    }
    public async Task<User?> GetUserById(int id)
    {
        return await _db.LoadSingleAsync<User, dynamic>("SELECT * FROM Users WHERE Id=@Id", new { Id = id });
    }
    public async Task<User?> GetByEmail(User User)
    {
        return await _db.LoadSingleAsync<User, dynamic>("SELECT * FROM Users WHERE Email=@Email", new { Email = User.Email });
    }

    public async Task<User> InsertUser(User user, User activityUser)
    {
        bool hasDuplicate = await this.CheckDuplicateEntry(user);
        if (!hasDuplicate)
        {
            user.PasswordSalt = new SecurityHelper().CreateSalt();
            user.Password = new SecurityHelper().CreatePasswordHash(user.Password, user.PasswordSalt);

            int id = await _db.Insert<User>(user);

            if (id != 0)
            {

                UserActivity log = new UserActivity(activityUser.Id, "User added:" + user.Email, LogActivity.Insert);
                await _userActivityService.InsertUserActivity(log);

                user.Id = id;
                return user;
            }
        }
        return null;
    }
    public async Task<User> UpdateUser(User user, User activityUser)
    {
        string sql = @"UPDATE Users SET Name=@Name, Role=@Role, IsActive=@IsActive WHERE Id=@Id";
        if(await _db.SaveData(sql, user))
        {
            UserActivity log = new UserActivity(activityUser.Id, "User updated: " + user.Email, LogActivity.Update);
            await _userActivityService.InsertUserActivity(log);
        }

        return user;
    }
    public async Task<bool> Delete(User obj, User activityUser)
    {
        string query = "DELETE FROM Users WHERE Id=@Id";
        int count = await _db.DeleteData<User, object>(query, new { obj.Id });

        if(count > 0)
        {
            UserActivity log = new UserActivity(activityUser.Id, "User deleted: " + (string.IsNullOrEmpty(obj.Email) ? obj.Id : obj.Email), LogActivity.Delete);
            await _userActivityService.InsertUserActivity(log);
        }

        return count > 0;
    }

    public async Task<bool> ChangePassword(User user, User activityUser)
    {
        user.PasswordSalt = new SecurityHelper().CreateSalt();
        user.Password = new SecurityHelper().CreatePasswordHash(user.Password, user.PasswordSalt);

        string sql = @"UPDATE Users SET Password=@Password,PasswordSalt=@PasswordSalt WHERE Id=@Id";
        bool isSuccess = await _db.SaveData(sql, user);

        if(isSuccess)
        {
            UserActivity log = new UserActivity(activityUser.Id, "Password updated: " + (string.IsNullOrEmpty(user.Email) ? user.Id : user.Email), LogActivity.Update);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }
    public async Task<bool> CheckDuplicateEntry(User user)
    {
        string query = "SELECT COUNT(1) Count FROM Users WHERE LOWER(Email)=LOWER(@Email)";
        return await _db.LoadSingleAsync<bool, object>(query, user);
    }
}

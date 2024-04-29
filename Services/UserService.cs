using BOL;
using Services.Helpers;

namespace Services;

public interface IUserService
{
    Task<bool> AuthenticateUser(User user);
    Task<User?> GetUser(User user);
    Task<User?> GetUserById(int id);
    Task<User> InsertUser(User user);
    Task<User> UpdateUser(User user);
    Task<bool> ChangePassword(User user);
    Task<bool> Delete(User user);
    Task<IEnumerable<User>> GetUsers();
}

public class UserService : IUserService
{
    private readonly ISqlDataAccess _db;
    public UserService(ISqlDataAccess db)
    {
        _db = db;
    }

    public async Task<bool> AuthenticateUser(User user)
    {
        var results = await _db.LoadData<User, dynamic>("SELECT * FROM Users WHERE Email=@Email AND Password=@Password", new { Email = user.Email, Password = user.Password });
        return results.Any();
    }
    public async Task<IEnumerable<User>> GetUsers() =>
        await _db.LoadData<User, dynamic>("SELECT * FROM Users", new { });


    public async Task<User?> GetUser(User user)
    {
        return await _db.LoadSingleAsync<User, dynamic>("SELECT * FROM Users WHERE Id=@Id OR Email=@Email", new { Id = user.Id, Email = user.Email });
    }
    public async Task<User?> GetUserById(int id)
    {
        return await _db.LoadSingleAsync<User, dynamic>("SELECT * FROM Users WHERE Id=@Id", new { Id = id });
    }

    public async Task<User> InsertUser(User user)
    {
        int id = await _db.Insert<User>(user);

        if (id != 0)
        {
            user.Id = id;
            return user;
        }

        return null;
    }

    public async Task<User> UpdateUser(User user)
    {
        string sql = @"UPDATE Users SET Name=@Name, Email=@Email, Role=@Role, Password=@Password WHERE Id=@Id";
        await _db.SaveData(sql, user);
        return user;
    }
    public async Task<bool> Delete(User obj)
    {
        string query = "DELETE FROM Users WHERE Id=@Id";
        int count = await _db.DeleteData<User, object>(query, new { obj.Id });
        return count > 0;
    }

    public async Task<bool> ChangePassword(User user)
    {
        string sql = @"UPDATE Users SET Password=@Password WHERE Id=@Id AND Email=@Email";
        return await _db.SaveData(sql, user);
    }
}

using BOL;
using Dapper;
using Services.Helpers;

namespace Services;

public interface IUserService
{
    Task<bool> AuthenticateUser(User user);
    Task<User?> GetUser(User user);
    Task<User?> GetUserById(int id);
    Task<User> InsertUser(User user);
    Task<User> UpdateUser(User user);
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
        var results = await _db.LoadData<User, dynamic>("SELECT * FROM Users WHERE Id=@Id OR Email=@Email", new { Id = user.Id, Email = user.Email });
        return results.FirstOrDefault();
    }
    public async Task<User?> GetUserById(int id)
    {
        var results = await _db.LoadData<User, dynamic>("SELECT * FROM Users WHERE Id=@Id", new { Id = id });
        return results.FirstOrDefault();
    }

    public async Task<User> InsertUser(User user)
    {
        //int id = await _db.Insert<User>(user);
        int id = 0;

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

}

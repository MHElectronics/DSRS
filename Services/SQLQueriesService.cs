using BOL;
using Microsoft.Extensions.Configuration;
using Services.Helpers;
using System.Data;
using System.Data.SqlClient;

namespace Services;
public interface ISQLQueriesService
{
    Task<IEnumerable<SQLQueries>> Get();
    Task<SQLQueries> GetById(SQLQueries sQLQueries);
    Task<DataTable> ExecuteSQLQuery(SQLQueries sQLSearch, Dictionary<string, object> parameters);
}
public class SQLQueriesService(IConfiguration config, ISqlDataAccess _db, IUserActivityService _userActivityService) : ISQLQueriesService
{
    private string _connectionString = config.GetConnectionString("AxleLoadDBDirectQuery");

    public async Task<IEnumerable<SQLQueries>> Get()
    {
        return await _db.LoadData<SQLQueries, dynamic>("SELECT * FROM SQLQueries", null);
    }
    public async Task<SQLQueries> GetById(SQLQueries sQLQueries)
    {
        return await _db.LoadSingleAsync<SQLQueries, dynamic>("SELECT * FROM SQLQueries WHERE Id=@Id", sQLQueries);
    }

    public async Task<DataTable> ExecuteSQLQuery(SQLQueries sQLSearch, Dictionary<string, object> parameters)
    {
        DataTable dataTable = new DataTable();

        if (string.IsNullOrWhiteSpace(sQLSearch.Query))
        {
            throw new ArgumentException("SQL query cannot be null or empty");
        }

        //Open Connection
        SqlConnection conn = new SqlConnection(_connectionString);
        SqlCommand cmd = new SqlCommand(sQLSearch.Query, conn);
        conn.Open();

        // Create data adapter
        SqlDataAdapter da = new SqlDataAdapter(cmd);

        if(parameters is not null && parameters.Any())
        {
            foreach(KeyValuePair<string, object> item in parameters)
            {
                cmd.Parameters.Add(new SqlParameter("@" + item.Key, item.Value));
            }
        }

        try
        {
            // Fill datatabel using data adapter
            da.Fill(dataTable);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            //Close and dispose connection
            conn.Close();
            da.Dispose();
        }

        return dataTable;
    }

    public async Task<bool> InsertSqlQuery(SQLQueries obj, User user)
    {
        string sql = @"INSERT INTO SQLQueries(Title,Description,Query,Parameters)
            VALUES (@Title,@Description,@Query,@Parameters)";
        bool isSuccess = await _db.SaveData<SQLQueries>(sql, obj);

        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "SQLQueries Id: " + obj.Id, LogActivity.Insert);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }
    public async Task<bool> UpdateSqlQuery(SQLQueries obj, User user)
    {
        string sql = "UPDATE SQLQueries SET Title=@Title,Description=@Description,Query=@Query,Parameters=@Parameters WHERE Id=@Id";
        bool isSuccess = await _db.SaveData<SQLQueries>(sql, obj);

        if (isSuccess)
        {
            UserActivity log = new UserActivity(user.Id, "SQLQueries Id: " + obj.Id, LogActivity.Update);
            await _userActivityService.InsertUserActivity(log);
        }

        return isSuccess;
    }

}

using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Services.Helpers;

public interface ISqlDataAccess
{
    void SetConnection(string connectionId);

    Task<IEnumerable<T>> LoadData<T, U>(string query, U parameters);
    Task<T> LoadSingleAsync<T, U>(string query, U parameters);
    Task<int> DeleteData<T, U>(string query, U parameters);
    Task SaveData<T>(string query, T parameters);
    Task<int> Insert<T>(T obj) where T : class;
}
public class SqlDataAccess : ISqlDataAccess
{
    private readonly IConfiguration _config;
    private string _connectionId;
    public SqlDataAccess(IConfiguration config)
    {
        _config = config;
        _connectionId = "AxleLoadDB";
    }
    public void SetConnection(string connectionId)
    {
        _connectionId = connectionId;
    }
    public async Task<IEnumerable<T>> LoadData<T, U>(string query, U parameters)
    {
       // string test = _config.GetConnectionString(_connectionId);
        using IDbConnection connection = new SqlConnection(_config.GetConnectionString(_connectionId));

        return await connection.QueryAsync<T>(query, parameters);
    }
    public async Task<T> LoadSingleAsync<T, U>(string query, U parameters)
    {
        using IDbConnection connection = new SqlConnection(_config.GetConnectionString(_connectionId));

        return await connection.QuerySingleAsync<T>(query, parameters);
    }
    public async Task<int> DeleteData<T, U>(string query, U parameters)
    {
        using IDbConnection connection = new SqlConnection(_config.GetConnectionString(_connectionId));

        return await connection.ExecuteAsync(query, parameters);
    }

    public async Task SaveData<T>(string query, T parameters)
    {
        try
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(_connectionId));
            await connection.ExecuteAsync(query, parameters);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public async Task<int> Insert<T>(T obj) where T : class
    {
        try
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(_connectionId));
            return await connection.InsertAsync(obj);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}

using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Services.Helpers;

public interface ISqlDataAccess
{
    Task<IEnumerable<T>> LoadData<T, U>(string query, U parameters);
    Task<T> LoadSingleAsync<T, U>(string query, U parameters);
    Task<int> DeleteData<T, U>(string query, U parameters);
    Task<bool> SaveData<T>(string query, T parameters);
    Task InsertDataTable(DataTable csvFileData, string destinationTableName);
    Task<int> Insert<T>(T obj) where T : class;
}
public class SqlDataAccess : ISqlDataAccess
{
    private string _connectionString;
    public SqlDataAccess(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("AxleLoadDB");
    }
    public async Task<IEnumerable<T>> LoadData<T, U>(string query, U parameters)
    {
       // string test = _config.GetConnectionString(_connectionId);
        using IDbConnection connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<T>(query, parameters);
    }
    public async Task<T> LoadSingleAsync<T, U>(string query, U parameters)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);

        return await connection.QuerySingleAsync<T>(query, parameters);
    }
    public async Task<int> DeleteData<T, U>(string query, U parameters)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);

        return await connection.ExecuteAsync(query, parameters);
    }

    public async Task<bool> SaveData<T>(string query, T parameters)
    {
        try
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            int count = await connection.ExecuteAsync(query, parameters);
            return count > 0;
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
            using IDbConnection connection = new SqlConnection(_connectionString);
            return await connection.InsertAsync(obj);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task InsertDataTable(DataTable csvFileData, string destinationTableName)
    {
        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(_connectionString))
        {
            bulkCopy.DestinationTableName = destinationTableName;

            foreach (var column in csvFileData.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ToString(), column.ToString());
            }

            await bulkCopy.WriteToServerAsync(csvFileData);
        }
    }
}

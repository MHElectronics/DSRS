using BOL;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Services;
public interface ISQLSearchService
{
    Task<DataTable> GetSQLSearch(SQLSearch sQLSearch, Dictionary<string, object> parameters);
}
public class SQLSearchService(IConfiguration config) : ISQLSearchService
{
    private string _connectionString = config.GetConnectionString("AxleLoadDBDirectQuery");

    public async Task<DataTable> GetSQLSearch(SQLSearch sQLSearch, Dictionary<string, object> parameters)
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
}

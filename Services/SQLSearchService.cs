using BOL;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using Services.Helpers;
using System.Data;
using System.Data.SqlClient;

namespace Services;
public interface ISQLSearchService
{
    Task<DataTable> GetSQLSearch(SQLSearch sQLSearch);
}
public class SQLSearchService : ISQLSearchService
{
    private string _connectionString;
    public SQLSearchService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("AxleLoadDBDirectQuery");
    }
    public async Task<DataTable> GetSQLSearch(SQLSearch sQLSearch)
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

using BOL;
using Services.Helpers;
using System.Data;

namespace Services;
public interface ISQLSearchService
{
    Task<DataTable> GetSQLSearch(SQLSearch sQLSearch);
}
public class SQLSearchService : ISQLSearchService
{
    private readonly ISqlDataAccess _db;
    public SQLSearchService(ISqlDataAccess db)
    {
        _db = db;
    }
    public async Task<DataTable> GetSQLSearch(SQLSearch sQLSearch)
    {
        DataTable dataTable = new DataTable();

        if (string.IsNullOrWhiteSpace(sQLSearch.Query))
        {
            throw new ArgumentException("SQL query cannot be null or empty");
        }

        var data = await _db.LoadData<dynamic, dynamic>(sQLSearch.Query, new { });
        if (data != null)
        {
            foreach (var item in data)
            {
                if (dataTable.Columns.Count == 0)
                {
                    foreach (var prop in item)
                    {
                        dataTable.Columns.Add(prop.Key);
                    }
                }
                var row = dataTable.NewRow();
                foreach (var prop in item)
                {
                    row[prop.Key] = prop.Value ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }
        }

        return dataTable;
    }

}

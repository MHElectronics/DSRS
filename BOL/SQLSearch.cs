
namespace BOL;
public class SQLSearch
{
    public string Query { get; set; }
    public List<SqlParameter> Parameters { get; set; } = new List<SqlParameter>();
}
public class SqlParameter
{
    public string Name { get; set; }
    public string Value { get; set; }
}
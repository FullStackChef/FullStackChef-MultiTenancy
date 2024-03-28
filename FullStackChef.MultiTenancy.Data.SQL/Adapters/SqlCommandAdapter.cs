using Microsoft.Data.SqlClient;

namespace FullStackChef.MultiTenancy.Data.SQL.Adapters;

public interface ISqlCommandAdapter
{
    Task ExecuteNonQueryAsync();
    string CommandText { get; set; }
    SqlParameterCollection Parameters { get; }
}
public class SqlCommandAdapter(SqlCommand sqlCommand) : ISqlCommandAdapter
{
    public string CommandText
    {
        get { return sqlCommand.CommandText; }
        set { sqlCommand.CommandText = value; }
    }
    public SqlParameterCollection Parameters => sqlCommand.Parameters;
    public Task ExecuteNonQueryAsync() => sqlCommand.ExecuteNonQueryAsync();
}

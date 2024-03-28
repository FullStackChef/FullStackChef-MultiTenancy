using Microsoft.Data.SqlClient;

namespace FullStackChef.MultiTenancy.Data.SQL.Adapters;

public interface ISqlConnectionAdapter
{
    SqlConnection Value { get; }
    ISqlCommandAdapter CreateCommand();
    ISqlConnectionAdapter Open();
}

public class SqlConnectionAdapter(SqlConnection connection) : ISqlConnectionAdapter
{
    public SqlConnectionAdapter(string connectionString) : this(new SqlConnection(connectionString)) { }

    public SqlConnection Value => connection;

    public ISqlCommandAdapter CreateCommand() => new SqlCommandAdapter(Value.CreateCommand());

    public ISqlConnectionAdapter Open()
    {
        connection.Open();
        return this;
    }
}


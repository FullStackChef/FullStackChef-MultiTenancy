using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace FullStackChef.MultiTenancy.Data.SQL.Adapters;

public interface IShardAdapter
{
    public Shard Value { get; }
    /// <summary>
    /// Opens a connection to the shard.
    /// </summary>
    /// <param name="connectionString">The connection string to use to open the connection.</param>
    /// <returns>A connection to the shard.</returns>
    ISqlConnectionAdapter OpenConnection(string connectionString);
}

/// <summary>
/// Adapter to wrap the sealed <see cref="Shard"/> class and allow for mocking in unit tests.
/// </summary>
public class ShardAdapter(Shard shard) : IShardAdapter
{
    public Shard Value => shard;

    /// <summary>
    /// Opens a connection to the shard.
    /// </summary>
    /// <param name="connectionString">The connection string to use to open the connection.</param>
    /// <returns>A connection to the shard.</returns>
    public ISqlConnectionAdapter OpenConnection(string connectionString) => new SqlConnectionAdapter(connectionString).Open();
}

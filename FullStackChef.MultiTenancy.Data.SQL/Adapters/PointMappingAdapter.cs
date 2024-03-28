using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace FullStackChef.MultiTenancy.Data.SQL.Adapters;

/// <summary>
/// Interface for an adapter that wraps a <see cref="PointMapping{TKey}"/> object.
/// </summary>
public interface IPointMappingAdapter<TKey>
{
    /// <summary>
    /// Gets the point associated with the <see cref="PointMapping{TKey}"/> object.
    /// </summary>
    TKey Value { get; }

    /// <summary>
    /// Gets the shard associated with the <see cref="PointMapping{TKey}"/> object.
    /// </summary>
    IShardAdapter Shard { get; }

    /// <summary>
    /// Opens a connection to the shard associated with the <see cref="PointMapping{TKey}"/> object.
    /// </summary>
    /// <param name="connectionString">The connection string to use for the connection.</param>
    /// <returns>A <see cref="SqlConnection"/> object representing the open connection.</returns>
    ISqlConnectionAdapter OpenConnection(string connectionString);
}

/// <summary>
/// Adapter class for the <see cref="PointMapping{T}"/> class.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PointMappingAdapter{T}"/> class.
/// </remarks>
/// <typeparam name="TKey">The type of the point mapping key.</typeparam>
/// <param name="pointMapping">The <see cref="PointMapping{T}"/> object to be wrapped.</param>
public class PointMappingAdapter<TKey>(PointMapping<TKey> pointMapping) : IPointMappingAdapter<TKey>
{
    /// <inheritdoc/>
    public TKey Value => pointMapping.Value;

    /// <inheritdoc/>
    public IShardAdapter Shard => new ShardAdapter(pointMapping.Shard);

    /// <inheritdoc/>
    public ISqlConnectionAdapter OpenConnection(string connectionString) => Shard.OpenConnection(connectionString);
}

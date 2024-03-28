using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace FullStackChef.MultiTenancy.Data.SQL.Adapters;

/// <summary>
/// Defines the interface for interacting with a list shard map.
/// </summary>
/// <typeparam name="TKey">The type of the sharding key.</typeparam>
public interface IListShardMapAdapter<TKey>
{
    /// <summary>
    /// Tries to get the mapping for a specified sharding key.
    /// </summary>
    /// <param name="shardingKey">The sharding key.</param>
    /// <param name="mapping">The mapping associated with the sharding key, if found.</param>
    /// <returns>True if the mapping is found, otherwise false.</returns>
    bool TryGetMappingForKey(TKey shardingKey, out IPointMappingAdapter<TKey>? mapping);

    /// <summary>
    /// Creates a point mapping for a specified point and shard.
    /// </summary>
    /// <param name="point">The point value.</param>
    /// <param name="shard">The shard to map the point to.</param>
    /// <returns>The created point mapping.</returns>
    IPointMappingAdapter<TKey> CreatePointMapping(TKey point, IShardAdapter shard);

    /// <summary>
    /// Creates a shard at the specified location.
    /// </summary>
    /// <param name="shardLocation">The location of the shard.</param>
    /// <returns>The created shard.</returns>
    IShardAdapter CreateShard(ShardLocation shardLocation);

    /// <summary>
    /// Tries to get a shard at the specified location.
    /// </summary>
    /// <param name="shardLocation">The location of the shard.</param>
    /// <param name="shard">The shard at the specified location, if found.</param>
    /// <returns>True if the shard is found, otherwise false.</returns>
    bool TryGetShard(ShardLocation shardLocation, out IShardAdapter? shard);

    /// <summary>
    /// Opens a SQL connection for a specified key using the given connection string.
    /// </summary>
    /// <param name="key">The key to use for opening the connection.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>An adapter for the opened SQL connection.</returns>
    ISqlConnectionAdapter OpenConnectionForKey(TKey key, string connectionString);
}

/// <summary>
/// Adapter for the ListShardMap class, allowing it to be used in a more testable manner.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ListShardMapAdapter class.
/// </remarks>
/// <typeparam name="TKey">The type of the sharding key.</typeparam>
/// <param name="listShardMap">The ListShardMap instance to wrap.</param>
public class ListShardMapAdapter<TKey>(ListShardMap<TKey> listShardMap) : IListShardMapAdapter<TKey>
{
    /// <inheritdoc/>
    public bool TryGetMappingForKey(TKey shardingKey, out IPointMappingAdapter<TKey>? mapping)=>
        (mapping = listShardMap.TryGetMappingForKey(shardingKey, out PointMapping<TKey> internalMapping) ?
               new PointMappingAdapter<TKey>(internalMapping) : default) != null;

    /// <inheritdoc/>
    public IPointMappingAdapter<TKey> CreatePointMapping(TKey point, IShardAdapter shard) =>
         new PointMappingAdapter<TKey>(listShardMap.CreatePointMapping(point, shard.Value));

    /// <inheritdoc/>
    public IShardAdapter CreateShard(ShardLocation shardLocation) => new ShardAdapter(listShardMap.CreateShard(shardLocation));

    /// <inheritdoc/>
    public bool TryGetShard(ShardLocation shardLocation, out IShardAdapter? shard)=>
        (shard = listShardMap.TryGetShard(shardLocation, out Shard internalShard) ?
        new ShardAdapter(internalShard) : default) != null;

    /// <inheritdoc/>
    public ISqlConnectionAdapter OpenConnectionForKey(TKey key, string connectionString) =>
        new SqlConnectionAdapter(listShardMap.OpenConnectionForKey(key, connectionString));
}

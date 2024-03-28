using FullStackChef.MultiTenancy.Data.SQL.Adapters;
using FullStackChef.MultiTenancy.Data.SQL.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FullStackChef.MultiTenancy.Data.SQL.Factory;

public interface IListShardMapAdapterFactory
{
    IListShardMapAdapter<Guid>? ShardMap { get; }
}

public class ListShardMapAdapterFactory(IShardMapManagerAdapter shardMapManagerAdapter, ILogger<IListShardMapAdapterFactory> logger, CatalogOptions options) : IListShardMapAdapterFactory
{
    private readonly IListShardMapAdapter<Guid>? _shardMap = GetShardMap(shardMapManagerAdapter, logger, options);

    /// <summary>
    /// The shard map for the service.
    /// </summary>
    public IListShardMapAdapter<Guid>? ShardMap => _shardMap;
    public ListShardMapAdapterFactory(IShardMapManagerAdapter shardMapManagerAdapter, ILogger<IListShardMapAdapterFactory> logger, IOptions<CatalogOptions> options) :
        this(shardMapManagerAdapter, logger, options.Value)
    { }

    private static IListShardMapAdapter<Guid>? GetShardMap(IShardMapManagerAdapter shardMapManagerAdapter, ILogger<IListShardMapAdapterFactory> logger, CatalogOptions options)
    {
        try
        {
            // check if shard map exists and if not, create it 
            return !shardMapManagerAdapter.TryGetListShardMap(options.CatalogDatabase, out IListShardMapAdapter<Guid>? sm)
                ? shardMapManagerAdapter.CreateListShardMap<Guid>(options.CatalogDatabase)
                : sm;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error in tenant management initialisation.");
            return null;
        }
    }
}

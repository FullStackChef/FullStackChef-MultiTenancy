using FullStackChef.MultiTenancy.Data.Provider;
using FullStackChef.MultiTenancy.Data.SQL.Adapters;
using FullStackChef.MultiTenancy.Data.SQL.Factory;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FullStackChef.MultiTenancy.Data.SQL.Services;

public interface ITenantManagementService
{
    /// <summary>
    /// Creates a new shard for the specified tenant.
    /// </summary>
    /// <param name="tenantName">The name of the tenant.</param>
    /// <param name="tenantServer">The server name for the tenant's database.</param>
    /// <param name="databaseServerPort">The port number for the tenant's database server.</param>
    /// <param name="servicePlan">The service plan for the tenant.</param>
    /// <returns>The new shard for the tenant, or null if the shard could not be created.</returns>
    IShardAdapter? CreateNewShard(string tenantName, string tenantServer, int databaseServerPort, string servicePlan);

    /// <summary>
    /// Registers a new shard for the specified tenant in the shard map.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant.</param>
    /// <param name="database">The name of the database for the tenant.</param>
    /// <param name="servicePlan">The service plan for the tenant.</param>
    /// <param name="shard">The shard for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value that indicates whether the registration was successful.</returns>
    Task<bool> RegisterNewShard(Guid tenantId, string database, string servicePlan, IShardAdapter shard);
    bool TryGetMappingForKey(Guid shardingKey, out IPointMappingAdapter<Guid>? mapping);
}


/// <summary>
/// Service that manages multi-tenant databases using the Elastic Scale Library.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TenantManagementService class.
/// </remarks>
/// <param name="catalogRepository">The repository for the tenant catalog.</param>
/// <param name="options">The options for the service.</param>
public class TenantManagementService(ITenantCatalogFactory catalogRepositoryFactory, IListShardMapAdapterFactory shardMapAdapterFactory, ILogger<ITenantManagementService> logger) : ITenantManagementService
{

    /// <inheritdoc/>
    public IShardAdapter? CreateNewShard(string database, string tenantServer, int databaseServerPort, string servicePlan)
    {
        if (shardMapAdapterFactory.ShardMap == null)
        {
            logger.LogError("No shard map was available for creating new shard");
            return null;
        }
        try
        {
            ShardLocation shardLocation = new(tenantServer, database, SqlProtocol.Tcp, databaseServerPort);
            if (!shardMapAdapterFactory.ShardMap.TryGetShard(shardLocation, out IShardAdapter? shard))
            {
                //create shard if it does not exist
                shard = shardMapAdapterFactory.ShardMap.CreateShard(shardLocation);
            }
            return shard;
        }
        catch (Exception exception)
        {
            logger.LogError(exception.Message, "Error in registering new shard.");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RegisterNewShard(Guid tenantId, string tenantName, string servicePlan, IShardAdapter shard)
    {
        if (shardMapAdapterFactory.ShardMap == null)
        {
            logger.LogError("No shard map was available for registering new shard");
            return false;
        }
        try
        {
            // Register the mapping of the tenant to the shard in the shard map.
            // After this step, DDR on the shard map can be used
            if (!shardMapAdapterFactory.ShardMap.TryGetMappingForKey(tenantId, out IPointMappingAdapter<Guid>? mapping))
            {
                IPointMappingAdapter<Guid> pointMapping = shardMapAdapterFactory.ShardMap.CreatePointMapping(tenantId, shard);


                //add tenant to Tenants table
                TenantRecord tenant = new(tenantId, tenantName, servicePlan);
                using ITenantCatalog _catalogRepository = catalogRepositoryFactory.CreateCatalog();
                await _catalogRepository.Add(tenant);
            }
            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(exception.Message, "Error in registering new shard.");
            return false;
        }
    }

    public bool TryGetMappingForKey(Guid shardingKey, out IPointMappingAdapter<Guid>? mapping) =>
        (mapping = (shardMapAdapterFactory.ShardMap != null &&
        shardMapAdapterFactory.ShardMap.TryGetMappingForKey(shardingKey, out mapping)) ? mapping : default) != null;

}

using FullStackChef.MultiTenancy.Data.Provider;
using FullStackChef.MultiTenancy.Data.SQL.Adapters;
using FullStackChef.MultiTenancy.Data.SQL.Exceptions;
using FullStackChef.MultiTenancy.Data.SQL.Factory;
using FullStackChef.MultiTenancy.Data.SQL.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FullStackChef.MultiTenancy.Data.SQL.Services;


/// <summary>
/// Service interface for creating database connections for tenants.
/// </summary>
public interface ITenantConnectionService
{
    /// <summary>
    /// Creates a database connection for a given sharding key.
    /// </summary>
    /// <param name="shardingKey">The sharding key for the tenant.</param>
    /// <returns>A <see cref="DbConnection"/> to the tenant's database.</returns>
    Task<ISqlConnectionAdapter> CreateDatabaseConnectionAsync(Guid shardingKey);
}

/// <summary>
/// Service for creating database connections for tenants.
/// </summary>
public class TenantConnectionService(
    ILogger<ITenantConnectionService> logger,
    ITenantManagementService tenantManagementService,
    IListShardMapAdapterFactory shardMapAdapterFactory,
    ITenantCatalogFactory catalogRepositoryFactory,
    IOptions<CatalogOptions> catalogOptions,
    IOptions<TenantOptions> tenantOptions) : ITenantConnectionService
{
    private readonly ILogger<ITenantConnectionService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITenantManagementService _tenantManagementService = tenantManagementService ?? throw new ArgumentNullException(nameof(tenantManagementService));
    private readonly IListShardMapAdapterFactory _shardMapAdapterFactory = shardMapAdapterFactory ?? throw new ArgumentNullException(nameof(shardMapAdapterFactory));
    private readonly ITenantCatalogFactory _catalogRepositoryFactory = catalogRepositoryFactory ?? throw new ArgumentNullException(nameof(catalogRepositoryFactory));
    private readonly CatalogOptions _catalogOptions = catalogOptions?.Value ?? throw new ArgumentNullException(nameof(catalogOptions));
    private readonly TenantOptions _tenantOptions = tenantOptions?.Value ?? throw new ArgumentNullException(nameof(tenantOptions));

    public async Task<ISqlConnectionAdapter> CreateDatabaseConnectionAsync(Guid shardingKey)
    {
        // Try to get the mapping for the given sharding key.
        if (!_tenantManagementService.TryGetMappingForKey(shardingKey, out _))
        {
            _logger.LogInformation("Mapping not found for sharding key: {shardingKey}.", shardingKey);

            using ITenantCatalog catalogRepository = _catalogRepositoryFactory.CreateCatalog();

            TenantRecord tenant = await catalogRepository.GetTenantById(shardingKey)
                ?? throw new TenantNotFoundException($"No tenant found with sharding key: {shardingKey}");

            IShardAdapter shard = _tenantManagementService.CreateNewShard(
                _tenantOptions.DatabaseName,
                _tenantOptions.DatabaseServer,
                _tenantOptions.DatabasePort,
                tenant.ServicePlan) ??
                throw new ShardNotFoundException($"Failed to create shard for tenant with sharding key {shardingKey}");

            // Register the shard for the tenant.
            if (!await _tenantManagementService.RegisterNewShard(tenant.TenantId, tenant.TenantName, tenant.ServicePlan, shard))
            {
                throw new ShardRegistrationFailedException($"Failed to register shard for tenant with sharding key {shardingKey}");
            }

            // Get the newly created shard mapping.
            if (!_tenantManagementService.TryGetMappingForKey(shardingKey, out _))
            {
                throw new ShardNotFoundException($"Failed to get shard for tenant with sharding key {shardingKey}");
            }

        }
        // Get the connection string from the mapping.
        ISqlConnectionAdapter? connection = (_shardMapAdapterFactory.ShardMap?.OpenConnectionForKey(shardingKey, _catalogOptions.ConnectionString))
            ?? throw new ApplicationException($"Failed to initialize a valid sql connection for tenant with sharding key {shardingKey}");

        // Set TenantId in SESSION_CONTEXT to shardingKey to enable Row-Level Security filtering
        ISqlCommandAdapter cmd = connection.CreateCommand();
        cmd.CommandText = @"exec sp_set_session_context @key=N'TenantId', @value=@shardingKey";
        cmd.Parameters.AddWithValue("@shardingKey", shardingKey);
        await cmd.ExecuteNonQueryAsync();

        return connection;
    }
}



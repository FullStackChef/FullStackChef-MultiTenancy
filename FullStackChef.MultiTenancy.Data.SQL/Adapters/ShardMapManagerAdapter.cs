using FullStackChef.MultiTenancy.Data.SQL.Options;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FullStackChef.MultiTenancy.Data.SQL.Adapters
{
    public interface IShardMapManagerAdapter
    {
        /// <summary>
        /// Creates a new ListShardMap with the specified name and generic type parameter T.
        /// </summary>
        /// <typeparam name="T">The generic type parameter for the ListShardMap.</typeparam>
        /// <param name="shardMapName">The name of the ListShardMap to create.</param>
        /// <returns>A new ListShardMap with the specified name and type parameter.</returns>
        IListShardMapAdapter<T> CreateListShardMap<T>(string shardMapName);

        /// <summary>
        /// Tries to retrieve a ListShardMap with the specified name and generic type parameter T.
        /// </summary>
        /// <typeparam name="T">The generic type parameter for the ListShardMap.</typeparam>
        /// <param name="shardMapName">The name of the ListShardMap to retrieve.</param>
        /// <param name="shardMap">The ListShardMap with the specified name and type parameter, if it exists.</param>
        /// <returns>True if the ListShardMap with the specified name and type parameter was retrieved, false otherwise.</returns>
        bool TryGetListShardMap<T>(string shardMapName, out IListShardMapAdapter<T>? shardMap);
    }

    /// <summary>
    /// Adapter for the ShardMapManager class from the Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement library.
    /// </summary>
    public class ShardMapManagerAdapter(CatalogOptions options) : IShardMapManagerAdapter
    {
        private readonly ShardMapManager _shardMapManager = GetShardMapManager(options);
        public ShardMapManagerAdapter(IOptions<CatalogOptions> options) : this(options.Value) { }

        /// <inheritdoc/>
        public IListShardMapAdapter<T> CreateListShardMap<T>(string shardMapName) =>
            new ListShardMapAdapter<T>(_shardMapManager.CreateListShardMap<T>(shardMapName));

        /// <inheritdoc/>
        public bool TryGetListShardMap<T>(string shardMapName, out IListShardMapAdapter<T>? shardMap) =>
            (shardMap = _shardMapManager.TryGetListShardMap(shardMapName, out ListShardMap<T> internalShardMap) ?
            new ListShardMapAdapter<T>(internalShardMap) : default) != null;


        private static ShardMapManager GetShardMapManager(CatalogOptions options)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new(options.ConnectionString)
            {
                InitialCatalog = options.CatalogDatabase,
                DataSource = options.CatalogServer
            };
            return ShardMapManagerFactory.TryGetSqlShardMapManager(connectionStringBuilder.ConnectionString,
                    ShardMapManagerLoadPolicy.Lazy, out ShardMapManager smm)
                    ? smm
                    : ShardMapManagerFactory.CreateSqlShardMapManager(connectionStringBuilder.ConnectionString);
        }
    }
}

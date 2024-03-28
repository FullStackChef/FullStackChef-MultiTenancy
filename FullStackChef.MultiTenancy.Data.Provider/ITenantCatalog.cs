namespace FullStackChef.MultiTenancy.Data.Provider;

public interface ITenantCatalog : IDisposable
{
    Task Add(TenantRecord tenant);
    Task<TenantRecord?> GetTenantById(Guid shardingKey);
}

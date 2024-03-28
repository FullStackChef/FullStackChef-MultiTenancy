namespace FullStackChef.MultiTenancy.Data.Provider;

public interface IMultiTenancyProvider
{
    Task<Guid> GetTenantIdAsync();
}

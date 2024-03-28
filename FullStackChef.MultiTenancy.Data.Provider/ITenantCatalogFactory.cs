namespace FullStackChef.MultiTenancy.Data.Provider;

public interface ITenantCatalogFactory
{
    ITenantCatalog CreateCatalog();
}

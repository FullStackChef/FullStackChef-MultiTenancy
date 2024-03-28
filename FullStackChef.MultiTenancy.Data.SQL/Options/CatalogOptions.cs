namespace FullStackChef.MultiTenancy.Data.SQL.Options;

public class CatalogOptions
{
    public required string CatalogServer { get; set; }
    public required string CatalogDatabase { get; set; }
    public required string ConnectionString { get; set; }
}

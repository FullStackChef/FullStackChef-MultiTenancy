namespace FullStackChef.MultiTenancy.Data.SQL.Options;

public class TenantOptions
{
    public required string DatabaseName { get; set; }
    public required string DatabaseServer { get; set; }
    public int DatabasePort { get; set; }
}

namespace FullStackChef.MultiTenancy.Data.Provider;

public record TenantRecord(
    Guid TenantId,
    string TenantName,
    string ServicePlan
    );


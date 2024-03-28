namespace FullStackChef.MultiTenancy.Data.SQL.Exceptions;

public class TenantNotFoundException(string? message) : Exception(message) {}

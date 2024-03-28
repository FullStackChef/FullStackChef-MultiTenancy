namespace FullStackChef.MultiTenancy.Data.SQL.Exceptions;

public class ShardNotFoundException(string? message) : Exception(message) { }

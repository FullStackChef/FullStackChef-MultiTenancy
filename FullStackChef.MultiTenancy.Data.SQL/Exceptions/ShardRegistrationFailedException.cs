namespace FullStackChef.MultiTenancy.Data.SQL.Exceptions
{
    public class ShardRegistrationFailedException(string? message) : Exception(message) { }
}

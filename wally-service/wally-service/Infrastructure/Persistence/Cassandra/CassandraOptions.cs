namespace WallyBallBackend.Infrastructure.Persistence.Cassandra;

public sealed class CassandraOptions
{
    public const string SectionName = "Cassandra";

    public string[] ContactPoints { get; set; } = ["localhost"];

    public int Port { get; set; } = 9042;

    public string Keyspace { get; set; } = "wallyball";
}

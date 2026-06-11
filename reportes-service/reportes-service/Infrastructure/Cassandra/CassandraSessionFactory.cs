using Cassandra;
using Microsoft.Extensions.Options;

namespace ReportesService.Infrastructure.Cassandra;

public sealed class CassandraSessionFactory : ICassandraSessionFactory
{
    private readonly ICluster _cluster;
    private readonly CassandraOptions _options;

    public CassandraSessionFactory(ICluster cluster, IOptions<CassandraOptions> options)
    {
        _cluster = cluster;
        _options = options.Value;
    }

    public Task<global::Cassandra.ISession> CreateSessionAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _cluster.ConnectAsync(_options.Keyspace);
    }
}

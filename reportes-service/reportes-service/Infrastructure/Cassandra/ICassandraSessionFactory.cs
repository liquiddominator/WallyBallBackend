namespace ReportesService.Infrastructure.Cassandra;

public interface ICassandraSessionFactory
{
    Task<global::Cassandra.ISession> CreateSessionAsync(CancellationToken cancellationToken = default);
}

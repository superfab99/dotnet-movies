using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Login.Data;

namespace Movies.Login.HealthChecks
{
    public sealed class MoviesLoginDatabaseHealthCheck : IHealthCheck
    {
        private readonly MoviesLoginDbContext _dbContext;

        public MoviesLoginDatabaseHealthCheck(MoviesLoginDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

                return canConnect
                    ? HealthCheckResult.Healthy("Movies Login database is reachable.")
                    : HealthCheckResult.Unhealthy("Movies Login database is not reachable.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Movies Login database check failed.", ex);
            }
        }
    }
}

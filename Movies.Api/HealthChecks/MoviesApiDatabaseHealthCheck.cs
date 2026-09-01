using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Api.Data;

namespace Movies.Api.HealthChecks
{
    public sealed class MoviesApiDatabaseHealthCheck : IHealthCheck
    {
        private readonly MoviesApiDbContext _dbContext;

        public MoviesApiDatabaseHealthCheck(MoviesApiDbContext dbContext)
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
                    ? HealthCheckResult.Healthy("Movies API database is reachable.")
                    : HealthCheckResult.Unhealthy("Movies API database is not reachable.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Movies API database check failed.", ex);
            }
        }
    }
}

using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Movies.Login.HealthChecks
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddMoviesLoginHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<MoviesLoginDatabaseHealthCheck>("movies-login-database", tags: ["ready"]);

            return services;
        }

        public static IEndpointRouteBuilder MapMoviesLoginHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("ready")
            });

            return endpoints;
        }
    }
}

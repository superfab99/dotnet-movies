using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Movies.Api.HealthChecks
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddMoviesApiHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<MoviesApiDatabaseHealthCheck>("movies-api-database", tags: ["ready"]);

            return services;
        }

        public static IEndpointRouteBuilder MapMoviesApiHealthChecks(this IEndpointRouteBuilder endpoints)
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

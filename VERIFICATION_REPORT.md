# Movies Services Verification Report

**Date:** September 6, 2026  
**Scope:** `Movies.Api`, `Movies.Login`, `Movies.Review`, and local Docker infrastructure

## Current Status

The repository contains three separate .NET 10 services with independent EF Core SQL Server databases:

| Service         | Responsibility                                         | Database         |
| --------------- | ------------------------------------------------------ | ---------------- |
| `Movies.Api`    | Movies, actors, posters, and movie-actor relationships | `MoviesApiDb`    |
| `Movies.Login`  | Users, roles, authentication, and refresh tokens       | `MoviesLoginDb`  |
| `Movies.Review` | Movie reviews                                          | `MoviesReviewDb` |

EF migrations and startup seeders are present in all three services. The local Docker Compose file starts SQL Server and RabbitMQ together.

## Verified Implemented Features

### Persistence and Database Setup

- EF Core SQL Server is configured in all three projects.
- Migrations exist for all three databases.
- `Movies.Api`, `Movies.Login`, and `Movies.Review` call `Database.MigrateAsync()` through their startup seeders.
- Seed data is inserted only when the target tables are empty:
  - `Movies.Api`: movies, actors, and movie-actor relationships.
  - `Movies.Login`: roles, users, and user-role links.
  - `Movies.Review`: sample reviews.
- Seed operations are intended to be repeatable without duplicating data.

### Movies.Api

- Movie CRUD endpoints are implemented.
- Pagination, title and genre filtering, and sorting are implemented.
- Actor and movie-actor models are implemented.
- The movie-actor many-to-many relationship uses a unique composite index.
- Movie poster upload and retrieval support is present.
- Generic and specialized repository layers are present.
- Service and mapping layers are present.
- In-memory caching is used for movie reads with invalidation after writes.
- Request logging middleware is present.
- Security response headers are present.
- Global exception handling is present.
- Health check endpoints are present.
- JWT bearer authentication and Swagger bearer configuration are present.
- Rate limiting is present.

### Movies.Login

- User registration and password hashing are implemented.
- User login and JWT access-token generation are implemented.
- Refresh-token rotation and revocation are implemented.
- Roles and user-role relationships are implemented.
- Claims and the `MovieWrite` authorization policy are configured.
- Startup seeding for roles and users is implemented.
- Health check endpoints are present.
- JWT bearer authentication and Swagger bearer configuration are present.
- Rate limiting and a structured `429` response are present.

### Movies.Review

- Review CRUD service, repository, controller, and DTO mapping are present.
- Reviews use a separate database from `Movies.Api`.
- Reviews store `MovieId` as an identifier owned by the API service; there is no cross-database EF foreign key.
- `MovieCreated` is consumed from RabbitMQ through a dedicated `movies-review-movie-created` queue.
- Consumed movies are stored as a local `Movie` projection in `MoviesReviewDb`.
- Movie projection writes are idempotent at the application level using `SourceMovieId`.
- Review creation validates that the movie exists in the local projection before saving.
- Unknown movie IDs return `404` from the review create endpoint.
- The `AddMovieProjection` migration creates the local `Movies` table.
- Startup migration and review seeding are present.
- JWT bearer authentication and Swagger bearer configuration are present.

### Docker Infrastructure

- `docker-compose.yml` defines SQL Server and RabbitMQ services.
- SQL Server is exposed on host port `1433`.
- RabbitMQ messaging is exposed on `5672`.
- RabbitMQ Management UI is exposed on `15672`.
- SQL Server and RabbitMQ data paths are declared as container volumes.
- SQL Server may run through Docker's `linux/amd64` emulation on an Apple Silicon Mac; the current Compose file reports this as a platform warning.

## Important Corrections To Earlier Claims

- SQL Server Serilog persistence is currently disabled. The MSSQL sink is commented out in `Movies.Api/Utils/Serilogs.cs`; console logging remains enabled.
- The old Movie-to-Review EF relationship is no longer valid. Reviews were removed from `Movies.Api` and moved to `Movies.Review`.
- `Movies.Review` is a separate service and is not covered by the older two-project verification statements.
- Only in-memory caching is configured. Redis or distributed caching is not configured.
- The report previously claimed 17 of 26 topics while listing 18 completed items; that metric has been removed because it was not reliable.
- The report previously described the system as production ready. It should currently be treated as a development-stage microservice solution until messaging, tests, secrets, and operational hardening are completed.

## Pending Work

### High Priority

1. **RabbitMQ reliability and synchronization**
   - Add MassTransit retry and error-queue handling for failed consumers.
   - Add the MassTransit outbox to `Movies.Api` so database writes and published events remain reliable together.
   - Add `MovieUpdated` and `MovieDeleted` consumers in `Movies.Review`.
   - Add a unique database index on `Movie.SourceMovieId` as a second line of defense against duplicate projections.
   - Decide how the review API represents the temporary period before a movie projection is replicated.

2. **Automated tests**
   - Add unit tests for service validation and authorization.
   - Add integration tests for authentication, migrations, CRUD, and seed behavior.
   - Add end-to-end tests for cross-service workflows.

3. **Configuration and secrets**
   - Move SQL and JWT secrets out of committed appsettings files.
   - Use environment variables, .NET user secrets, or a secret store.
   - Add explicit Development, Staging, and Production configuration.

4. **Build and runtime verification**
   - Run a clean build for all three projects.
   - Start all services against the recreated SQL Server databases.
   - Verify migrations, seed data, health checks, Swagger, and authentication.

### Medium Priority

- Add health checks to `Movies.Review`.
- Add consistent exception handling and request logging to `Movies.Review`.
- Add consistent rate limiting and security headers across all services.
- Add service-to-service authentication and authorization.
- Add distributed tracing and correlation IDs.
- Add resilience policies for HTTP and message-broker communication.
- Decide whether an API gateway is needed.
- Add API versioning and contract compatibility rules.
- Review indexes and query performance as data volume grows.
- Add CORS policy where browser clients require it.

### Learning and Architecture Topics

- One-to-one relationship modeling.
- Cross-service ownership and eventual consistency.
- RabbitMQ exchanges, queues, routing, acknowledgements, retries, and dead-lettering.
- MassTransit consumers, retries, outbox, and idempotency.
- Containerizing the three application services, not only their infrastructure.
- Deployment pipelines and Kubernetes fundamentals.

## Verification Commands

From the repository root:

```bash
dotnet build Movies.Api/Movies.Api.csproj
dotnet build Movies.Login/Movies.Login.csproj
dotnet build Movies.Review/Movies.Review.csproj
docker compose config
docker compose ps
```

Apply migrations explicitly when needed:

```bash
dotnet ef database update --project Movies.Api --startup-project Movies.Api --context MoviesApiDbContext
dotnet ef database update --project Movies.Login --startup-project Movies.Login --context MoviesLoginDbContext
dotnet ef database update --project Movies.Review --startup-project Movies.Review --context MoviesReviewDbContext
```

## Overall Assessment

The project has a solid multi-service foundation: independent databases, EF migrations, startup seeding, authentication, CRUD workflows, shared event contracts, MassTransit publishing, a RabbitMQ consumer, and a local movie projection are present. The current design is eventually consistent: a movie may be created in `Movies.Api` before it is available in `Movies.Review`. Consumer retries, outbox handling, update/delete synchronization, automated tests, secrets management, and consistent cross-service operational concerns remain before production deployment.

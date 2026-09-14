using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MassTransit;
using Movies.Review.Data;
using Movies.Review.Mappings;
using Movies.Review.Consumers;
using Movies.Review.Repositories;
using Movies.Review.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddAuthorization();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role
        };
    });

    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token."
        });
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
        {
        new OpenApiSecuritySchemeReference("Bearer", document),
        new List<string>()
        }
        });
    });

    builder.Services.AddDbContext<MoviesReviewDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddMassTransit(x =>
    {
        // automatic registration
        x.AddConsumer<MovieCreatedConsumer>();
        x.AddConsumer<MovieDeletedConsumer>();
        x.AddConsumer<MovieUpdatedConsumer>();

        // Applies retry then redelivery to manually declared receive endpoints.
        void ApplyResilience(IRabbitMqReceiveEndpointConfigurator endpoint)
        {
            // Longer outages use RabbitMQ's delayed-message-exchange plugin.
            endpoint.UseDelayedRedelivery(r => r.Intervals(
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(15)));

            // Short transient failures use quick in-memory retries first.
            endpoint.UseMessageRetry(r => r.Exponential(
                5,
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromMilliseconds(200)));
        }

        x.UsingRabbitMq((context, configurator) =>
        {
            var rabbitHost = builder.Configuration["RabbitMQSettings:Host"]
                ?? throw new InvalidOperationException("RabbitMQ host is not configured.");
            var rabbitUsername = builder.Configuration["RabbitMQSettings:Username"]
                ?? throw new InvalidOperationException("RabbitMQ username is not configured.");
            var rabbitPassword = builder.Configuration["RabbitMQSettings:Password"]
                ?? throw new InvalidOperationException("RabbitMQ password is not configured.");

            configurator.Host(rabbitHost, host =>
            {
                host.Username(rabbitUsername);
                host.Password(rabbitPassword);
            });

            // only required if you want to provide explicit queue names
            configurator.ReceiveEndpoint(
            "movies-review-movie-created",
            endpoint =>
            {
                ApplyResilience(endpoint);
                endpoint.ConfigureConsumer<MovieCreatedConsumer>(context);
            });

            configurator.ReceiveEndpoint(
            "movies-review-movie-deleted",
            endpoint =>
            {
                ApplyResilience(endpoint);
                endpoint.ConfigureConsumer<MovieDeletedConsumer>(context);
            });

            configurator.ReceiveEndpoint(
            "movies-review-movie-updated",
            endpoint =>
            {
                ApplyResilience(endpoint);
                endpoint.ConfigureConsumer<MovieUpdatedConsumer>(context);
            });
        });
    });

    builder.Services.AddScoped<IReviewsService, ReviewsService>();
    builder.Services.AddScoped<IReviewsRepository, ReviewsRepository>();
    builder.Services.AddScoped<IMoviesRepository, MoviesRepository>();
    builder.Services.AddAutoMapper(_ => { }, typeof(ReviewMapping).Assembly);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await DbSeeder.SeedAsync(app.Services);

    app.Run();

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Movies.Api.Data;
using Movies.Api.Exceptions;
using Movies.Api.HealthChecks;
using Movies.Api.Mappings;
using Movies.Api.Middleware;
using Movies.Api.Repositories;
using Movies.Api.Services;
using Movies.Api.Utils;
using Serilog;

Log.Logger = new LoggerConfiguration()
   .WriteTo.Console()
   .CreateLogger();

try
{
    Log.Information("Starting Web Application");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.ConfigureSerilog();

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddMemoryCache();

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

    builder.Services.AddDbContext<MoviesApiDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddMoviesApiHealthChecks();

    builder.Services.AddScoped<IMoviesRepository, MoviesRepository>();
    builder.Services.AddScoped<IMoviesService, MoviesService>();
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IMoviePosterRepository, MoviePosterRepository>();
    builder.Services.AddScoped<IMoviePosterService, MoviePosterService>();
    builder.Services.AddScoped<IActorService, ActorService>();
    builder.Services.AddScoped<IActorRepository, ActorRepository>();
    builder.Services.AddScoped<IMovieActorService, MovieActorService>();


    //other mapping will be discovered if its in same folder
    builder.Services.AddAutoMapper(_ => { }, typeof(MovieMapping).Assembly);
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("movieslimit", context =>
        {
            var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });
    });

    var app = builder.Build();

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.UseStaticFiles();

    app.MapMoviesApiHealthChecks();

    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        await next();
    });

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Unable to start web application check logs");
}
finally
{
    Log.CloseAndFlush();
}

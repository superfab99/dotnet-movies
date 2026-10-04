using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Movies.Review.Data;
using Movies.Review.Mappings;
using Movies.Review.Repositories;
using Movies.Review.Services;
using Azure.Identity;
using Azure.Messaging.ServiceBus;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddAuthorization();

    var keyVaultUri = builder.Configuration["KeyVault:Uri"];
    if (string.IsNullOrWhiteSpace(keyVaultUri))
    {
        throw new InvalidOperationException("Key Vault URI is not configured.");
    }

    try
    {
        var credential = new AzureCliCredential();
        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);
        Console.WriteLine($"✓ Key Vault loaded successfully from: {keyVaultUri}");
        Console.WriteLine($"✓ JWT:Key = {builder.Configuration["Jwt:Key"]}");
        Console.WriteLine($"✓ ConnectionString = {builder.Configuration["ConnectionStrings:MoviesReviewDbConnection"]}");
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(ex.Message);
    }

    var serviceBusConnectionString = builder.Configuration["AzureServiceBus:ConnectionString"];
    builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));

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
            options.UseSqlServer(builder.Configuration.GetConnectionString("MoviesReviewDbConnection")));

    builder.Services.AddScoped<IReviewsService, ReviewsService>();
    builder.Services.AddScoped<IReviewsRepository, ReviewsRepository>();
    builder.Services.AddScoped<IMoviesRepository, MoviesRepository>();
    builder.Services.AddAutoMapper(_ => { }, typeof(ReviewMapping).Assembly);
    builder.Services.AddHostedService<ServiceBusConsumer>();

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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Movies.Login.Models;

namespace Movies.Login.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<MoviesLoginDbContext>();

        await dbContext.Database.MigrateAsync();

        var roles = new[]
        {
            new Roles { Name = "Admin" },
            new Roles { Name = "Manager" },
            new Roles { Name = "User" },
            new Roles { Name = "Guest" },
            new Roles { Name = "ProUser" }
        };

        foreach (var role in roles)
        {
            var existingRole = await dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == role.Name);

            if (existingRole == null)
            {
                dbContext.Roles.Add(role);
            }
        }

        await dbContext.SaveChangesAsync();

        var savedRoles = await dbContext.Roles
            .ToDictionaryAsync(r => r.Name);

        var passwordHasher = new PasswordHasher<User>();

        var users = new[]
        {
            new
            {
                Username = "admin",
                Email = "admin@example.com",
                FirstName = "System",
                LastName = "Administrator",
                Password = "Admin@123",
                RoleName = "Admin"
            },
            new
            {
                Username = "manager",
                Email = "manager@example.com",
                FirstName = "Movie",
                LastName = "Manager",
                Password = "Manager@123",
                RoleName = "Manager"
            },
            new
            {
                Username = "user",
                Email = "user@example.com",
                FirstName = "普通",
                LastName = "User",
                Password = "User@123",
                RoleName = "User"
            }
        };

        foreach (var seedUser in users)
        {
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == seedUser.Username);

            if (user == null)
            {
                user = new User
                {
                    Username = seedUser.Username,
                    Email = seedUser.Email,
                    FirstName = seedUser.FirstName,
                    LastName = seedUser.LastName,
                    PasswordHash = string.Empty
                };

                user.PasswordHash = passwordHasher.HashPassword(
                    user,
                    seedUser.Password);

                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
            }

            var role = savedRoles[seedUser.RoleName];

            var userRoleExists = await dbContext.UserRoles.AnyAsync(
                ur => ur.UserId == user.Id && ur.RoleId == role.Id);

            if (!userRoleExists)
            {
                dbContext.UserRoles.Add(new UserRoles
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }
}